using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.TeacherPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        TeacherScheduleController
        =====================================================

        هذا الكنترولر مسؤول عن عرض الجدول الدراسي للأستاذ.

        الأستاذ لا يضيف ولا يعدل ولا يحذف من هنا.
        فقط يشاهد جدوله حسب التكليفات المرتبطة به.
    */
    public class TeacherScheduleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TeacherScheduleController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            التحقق من دخول الأستاذ

            نحتاج:
            - Token
            - RoleName = Teacher
            - TeacherId
        */
        private bool IsTeacherLoggedIn(out string token, out int teacherId)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            teacherId = 0;

            var teacherIdFromInt = HttpContext.Session.GetInt32("TeacherId");

            if (teacherIdFromInt.HasValue)
            {
                teacherId = teacherIdFromInt.Value;
            }
            else
            {
                var teacherIdText = HttpContext.Session.GetString("TeacherId");
                int.TryParse(teacherIdText, out teacherId);
            }

            return !string.IsNullOrWhiteSpace(token)
                   && roleName == "Teacher"
                   && teacherId > 0;
        }

        /*
            تجهيز HttpClient مع التوكن
        */
        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /*
            جلب تكليفات الأستاذ

            نحتاج أرقام التكليفات حتى نعرض فقط الحصص المرتبطة بها.
        */
        private async Task<TeacherAssignmentsPageViewModel> GetTeacherAssignmentsPage(string token, int teacherId)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-teacher-assignments-by-teacher/{teacherId}");

            if (!response.IsSuccessStatusCode)
            {
                return new TeacherAssignmentsPageViewModel();
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TeacherAssignmentsPageViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new TeacherAssignmentsPageViewModel();
        }

        /*
            =====================================================
            عرض جدول الأستاذ
            =====================================================

            طريقة العمل:
            1) نأخذ TeacherId من Session
            2) نجيب تكليفات الأستاذ
            3) نجيب كل الجدول الدراسي
            4) نعرض فقط الحصص التي TeacherAssignmentId الخاص بها ضمن تكليفات الأستاذ
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var assignmentsPage = await GetTeacherAssignmentsPage(token, teacherId);

            var teacherAssignmentIds = assignmentsPage.Assignments
                .Select(a => a.Id)
                .ToList();

            if (!teacherAssignmentIds.Any())
            {
                ViewBag.TeacherName = assignmentsPage.TeacherName;
                ViewBag.TeacherNumber = assignmentsPage.TeacherNumber;
                ViewBag.Error = "لا توجد تكليفات تدريسية مرتبطة بك حتى الآن.";

                return View(new List<TeacherScheduleViewModel>());
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-school-schedules");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.TeacherName = assignmentsPage.TeacherName;
                ViewBag.TeacherNumber = assignmentsPage.TeacherNumber;
                ViewBag.Error = "تعذر تحميل الجدول الدراسي.";

                return View(new List<TeacherScheduleViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var schedules = JsonSerializer.Deserialize<List<TeacherScheduleViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TeacherScheduleViewModel>();

            /*
                نعرض فقط الحصص الخاصة بتكليفات الأستاذ الحالي
            */
            schedules = schedules
                .Where(s => teacherAssignmentIds.Contains(s.TeacherAssignmentId))
                .ToList();

            /*
                ترتيب الأيام يدويًا
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

            ViewBag.TeacherName = assignmentsPage.TeacherName;
            ViewBag.TeacherNumber = assignmentsPage.TeacherNumber;

            return View(schedules);
        }
    }
}