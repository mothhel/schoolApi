using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentLessonController
        =====================================================

        هذا الكنترولر مسؤول عن عرض دروس الطالب.

        الطالب لا يضيف ولا يعدل ولا يحذف.
        فقط يشاهد الدروس الخاصة بصفه وشعبته.

        عند فتح صفحة الدروس:
        يتم تصفير عداد الدروس الجديدة في لوحة الطالب.

        مهم:
        تصفير العداد لا يحذف الدروس.
        فقط يخبر النظام أن الطالب شاهد الدروس الحالية.
    */
    public class StudentLessonController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentLessonController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من دخول الطالب
            =====================================================

            نحتاج:
            - Token
            - RoleName = Student
            - StudentId
        */
        private bool IsStudentLoggedIn(out string token, out int studentId)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            studentId = 0;

            var studentIdFromInt = HttpContext.Session.GetInt32("StudentId");

            if (studentIdFromInt.HasValue)
            {
                studentId = studentIdFromInt.Value;
            }
            else
            {
                var studentIdText = HttpContext.Session.GetString("StudentId");
                int.TryParse(studentIdText, out studentId);
            }

            return !string.IsNullOrWhiteSpace(token)
                   && roleName == "Student"
                   && studentId > 0;
        }

        /*
            =====================================================
            تجهيز HttpClient مع التوكن
            =====================================================
        */
        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /*
            =====================================================
            جلب بيانات الطالب الحالي
            =====================================================

            نحتاج بيانات الطالب حتى نعرف:
            - الصف
            - الشعبة

            وبعدها نفلتر الدروس على نفس الصف والشعبة.
        */
        private async Task<StudentDashboardViewModel?> GetCurrentStudent(string token, int studentId)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StudentDashboardViewModel>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        /*
            =====================================================
            تعليم قسم الدروس بأنه تمت مشاهدته
            =====================================================

            هذا الرابط يعتمد على التوكن.
            لذلك لا نرسل StudentId في الرابط.

            API المستخدم:
            api/Student/mark-my-area-as-viewed/Lessons
        */
        private async Task MarkLessonsAsViewed(HttpClient client)
        {
            try
            {
                var response = await client.PostAsync(
                    "api/Student/mark-my-area-as-viewed/Lessons",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                        لا نوقف عرض الدروس إذا فشل تصفير العداد.
                        لأن عرض الدروس أهم من العداد.
                    */
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"فشل تصفير عداد الدروس: {response.StatusCode} - {error}");
                }
            }
            catch
            {
                /*
                    لا نوقف صفحة الدروس إذا حدث خطأ في تصفير العداد.
                */
            }
        }

        /*
            =====================================================
            عرض دروس الطالب
            =====================================================

            طريقة العمل:
            1) نتحقق من دخول الطالب.
            2) نجيب بيانات الطالب.
            3) نجيب كل الدروس من API.
            4) نعرض فقط الدروس التي تطابق صف وشعبة الطالب.
            5) نعتبر أن الطالب شاهد الدروس الحالية، فيختفي عداد "جديد".
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsStudentLoggedIn(out string token, out int studentId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await GetCurrentStudent(token, studentId);

            if (student == null)
            {
                ViewBag.Error = "تعذر تحميل بيانات الطالب.";
                return View(new List<StudentLessonViewModel>());
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-lessons");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الدروس.";
                return View(new List<StudentLessonViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var lessons = JsonSerializer.Deserialize<List<StudentLessonViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<StudentLessonViewModel>();

            /*
                نعرض فقط دروس صف وشعبة الطالب
            */
            lessons = lessons
                .Where(l =>
                    string.Equals(l.ClassRoomName?.Trim(), student.ClassRoomName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(l.SectionName?.Trim(), student.SectionName?.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.LessonDate)
                .ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.ClassRoomName = student.ClassRoomName;
            ViewBag.SectionName = student.SectionName;

            /*
                بعد تحميل الدروس الخاصة بالطالب،
                نعتبرها تمت مشاهدتها.
                لذلك سيتم تصفير عداد الدروس الجديدة.
            */
            await MarkLessonsAsViewed(client);

            return View(lessons);
        }
    }
}