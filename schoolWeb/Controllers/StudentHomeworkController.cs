using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentHomeworkController
        =====================================================

        هذا الكنترولر مسؤول عن عرض واجبات الطالب.

        الطالب لا يضيف ولا يعدل ولا يحذف.
        فقط يشاهد الواجبات الخاصة بصفه وشعبته.

        عند فتح صفحة الواجبات:
        يتم تصفير عداد الواجبات الجديدة في لوحة الطالب.

        مهم:
        تصفير العداد لا يحذف الواجبات.
        فقط يخبر النظام أن الطالب شاهد الواجبات الحالية.
    */
    public class StudentHomeworkController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentHomeworkController(IHttpClientFactory httpClientFactory)
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

            وبعدها نفلتر الواجبات على نفس الصف والشعبة.
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
            تعليم قسم الواجبات بأنه تمت مشاهدته
            =====================================================

            هذا الرابط يعتمد على التوكن.
            لذلك لا نرسل StudentId في الرابط.

            API المستخدم:
            api/Student/mark-my-area-as-viewed/Homeworks
        */
        private async Task MarkHomeworksAsViewed(HttpClient client)
        {
            try
            {
                var response = await client.PostAsync(
                    "api/Student/mark-my-area-as-viewed/Homeworks",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                        لا نوقف عرض الواجبات إذا فشل تصفير العداد.
                        لأن عرض الواجبات أهم من العداد.
                    */
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"فشل تصفير عداد الواجبات: {response.StatusCode} - {error}");
                }
            }
            catch
            {
                /*
                    لا نوقف صفحة الواجبات إذا حدث خطأ في تصفير العداد.
                */
            }
        }

        /*
            =====================================================
            عرض واجبات الطالب
            =====================================================

            طريقة العمل:
            1) نتحقق من دخول الطالب.
            2) نجيب بيانات الطالب.
            3) نجيب كل الواجبات من API.
            4) نعرض فقط الواجبات التي تطابق صف وشعبة الطالب.
            5) نعتبر أن الطالب شاهد الواجبات الحالية، فيختفي عداد "جديد".
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
                return View(new List<StudentHomeworkViewModel>());
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-homeworks");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الواجبات.";
                return View(new List<StudentHomeworkViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var homeworks = JsonSerializer.Deserialize<List<StudentHomeworkViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<StudentHomeworkViewModel>();

            /*
                نعرض فقط واجبات صف وشعبة الطالب
            */
            homeworks = homeworks
                .Where(h =>
                    string.Equals(h.ClassRoomName?.Trim(), student.ClassRoomName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(h.SectionName?.Trim(), student.SectionName?.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(h => h.DueDate)
                .ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.ClassRoomName = student.ClassRoomName;
            ViewBag.SectionName = student.SectionName;

            /*
                بعد تحميل الواجبات الخاصة بالطالب،
                نعتبرها تمت مشاهدتها.
                لذلك سيتم تصفير عداد الواجبات الجديدة.
            */
            await MarkHomeworksAsViewed(client);

            return View(homeworks);
        }
    }
}