using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentScheduleController
        =====================================================

        هذا الكنترولر مسؤول عن عرض الجدول الدراسي للطالب.

        الطالب لا يضيف ولا يعدل ولا يحذف.
        فقط يشاهد جدول صفه وشعبته.

        عند فتح صفحة الجدول الدراسي:
        يتم تصفير عداد الجدول الدراسي الجديد في لوحة الطالب.

        مهم:
        تصفير العداد لا يحذف الجدول الدراسي.
        فقط يخبر النظام أن الطالب شاهد الجدول الحالي.
    */
    public class StudentScheduleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentScheduleController(IHttpClientFactory httpClientFactory)
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
            تجهيز الاتصال مع API
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

            نحتاج:
            - ClassRoomId
            - SectionId

            حتى نطلب جدول الصف والشعبة من API.
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
            تعليم قسم الجدول الدراسي بأنه تمت مشاهدته
            =====================================================

            هذا الرابط يعتمد على التوكن.
            لذلك لا نرسل StudentId في الرابط.

            API المستخدم:
            api/Student/mark-my-area-as-viewed/Schedule
        */
        private async Task MarkScheduleAsViewed(HttpClient client)
        {
            try
            {
                var response = await client.PostAsync(
                    "api/Student/mark-my-area-as-viewed/Schedule",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                        لا نوقف عرض الجدول الدراسي إذا فشل تصفير العداد.
                        لأن عرض الجدول أهم من العداد.
                    */
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"فشل تصفير عداد الجدول الدراسي: {response.StatusCode} - {error}");
                }
            }
            catch
            {
                /*
                    لا نوقف صفحة الجدول الدراسي إذا حدث خطأ في تصفير العداد.
                */
            }
        }

        /*
            =====================================================
            عرض الجدول الدراسي للطالب
            =====================================================

            طريقة العمل:
            1) نتحقق من دخول الطالب.
            2) نجيب بيانات الطالب.
            3) نستخدم ClassRoomId و SectionId.
            4) نجيب جدول هذا الصف وهذه الشعبة فقط.
            5) نعتبر أن الطالب شاهد الجدول الحالي، فيختفي عداد "جديد".
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
                return View(new List<StudentScheduleViewModel>());
            }

            if (!student.ClassRoomId.HasValue || !student.SectionId.HasValue)
            {
                ViewBag.Error = "بيانات الصف أو الشعبة غير مكتملة لهذا الطالب.";
                return View(new List<StudentScheduleViewModel>());
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync(
                $"api/Admin/get-school-schedules-by-class-section?classRoomId={student.ClassRoomId.Value}&sectionId={student.SectionId.Value}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الجدول الدراسي.";
                return View(new List<StudentScheduleViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var schedules = JsonSerializer.Deserialize<List<StudentScheduleViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<StudentScheduleViewModel>();

            /*
                ترتيب الأيام يدويًا حتى لا تظهر أبجديًا.
            */
            var dayOrder = new Dictionary<string, int>
            {
                { "السبت", 1 },
                { "الأحد", 2 },
                { "الاثنين", 3 },
                { "الثلاثاء", 4 },
                { "الأربعاء", 5 },
                { "الخميس", 6 }
            };

            schedules = schedules
                .OrderBy(s => dayOrder.ContainsKey(s.DayName) ? dayOrder[s.DayName] : 99)
                .ThenBy(s => s.PeriodNumber)
                .ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.ClassRoomName = student.ClassRoomName;
            ViewBag.SectionName = student.SectionName;

            /*
                بعد تحميل الجدول الخاص بالطالب،
                نعتبره تمت مشاهدته.
                لذلك سيتم تصفير عداد الجدول الدراسي الجديد.
            */
            await MarkScheduleAsViewed(client);

            return View(schedules);
        }
    }
}