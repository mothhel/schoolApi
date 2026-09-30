using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentAttendanceController
        =====================================================

        هذا الكنترولر مسؤول عن عرض حضور وغياب الطالب.

        الطالب يشاهد سجله فقط:
        - حاضر
        - غائب
        - مستأذن

        لا يستطيع الإضافة أو التعديل أو الحذف.

        عند فتح صفحة الحضور والغياب:
        يتم تصفير عداد الحضور والغياب الجديد في لوحة الطالب.

        مهم:
        تصفير العداد لا يحذف سجلات الحضور والغياب.
        فقط يخبر النظام أن الطالب شاهد السجلات الحالية.
    */
    public class StudentAttendanceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentAttendanceController(IHttpClientFactory httpClientFactory)
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

            نحتاجها لعرض:
            - اسم الطالب
            - الصف
            - الشعبة
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
            تعليم قسم الحضور والغياب بأنه تمت مشاهدته
            =====================================================

            هذا الرابط يعتمد على التوكن.
            لذلك لا نرسل StudentId في الرابط.

            API المستخدم:
            api/Student/mark-my-area-as-viewed/Attendance
        */
        private async Task MarkAttendanceAsViewed(HttpClient client)
        {
            try
            {
                var response = await client.PostAsync(
                    "api/Student/mark-my-area-as-viewed/Attendance",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    /*
                        لا نوقف عرض الحضور والغياب إذا فشل تصفير العداد.
                        لأن عرض السجلات أهم من العداد.
                    */
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"فشل تصفير عداد الحضور والغياب: {response.StatusCode} - {error}");
                }
            }
            catch
            {
                /*
                    لا نوقف صفحة الحضور والغياب إذا حدث خطأ في تصفير العداد.
                */
            }
        }

        /*
            =====================================================
            عرض حضور وغياب الطالب
            =====================================================

            طريقة العمل:
            1) نتحقق من دخول الطالب.
            2) نجيب بيانات الطالب.
            3) نطلب سجل الحضور الخاص بهذا الطالب فقط من API.
            4) نرتب السجل من الأحدث إلى الأقدم.
            5) نعتبر أن الطالب شاهد سجلات الحضور والغياب الحالية.
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
                return View(new List<StudentAttendanceViewModel>());
            }

            var client = CreateClientWithToken(token);

            /*
                هذا API يرجع حضور وغياب الطالب الحالي فقط:
                api/Admin/get-student-attendance/{studentId}
            */
            var response = await client.GetAsync($"api/Admin/get-student-attendance/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل سجل الحضور والغياب.";
                return View(new List<StudentAttendanceViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var attendances = JsonSerializer.Deserialize<List<StudentAttendanceViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<StudentAttendanceViewModel>();

            attendances = attendances
                .OrderByDescending(a => a.AttendanceDate)
                .ToList();

            ViewBag.StudentName = student.FullName;
            ViewBag.ClassRoomName = student.ClassRoomName;
            ViewBag.SectionName = student.SectionName;

            /*
                بعد تحميل سجلات الحضور والغياب الخاصة بالطالب،
                نعتبرها تمت مشاهدتها.
                لذلك سيتم تصفير عداد الحضور والغياب الجديد.
            */
            await MarkAttendanceAsViewed(client);

            return View(attendances);
        }
    }
}