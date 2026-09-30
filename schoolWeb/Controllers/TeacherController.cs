using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.TeacherPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        TeacherController
        =====================================================

        هذا الكنترولر مسؤول عن واجهات المدرس.

        في هذه المرحلة:
        - نعرض لوحة المدرس
        - نعرض التكليفات الخاصة به
        - نعرض عدد حصص اليوم حسب الجدول الدراسي

        ملاحظة:
        المدرس لا يدخل درجات
        والمدرس لا يسجل حضور وغياب
        حسب قرارنا في المشروع.
    */
    public class TeacherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TeacherController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من أن المستخدم الحالي مدرس
            =====================================================

            نحتاج:
            - Token
            - RoleName = Teacher
            - TeacherId

            TeacherId يكون محفوظًا في Session بعد تسجيل الدخول.
        */
        private bool IsTeacherLoggedIn(out string token, out int teacherId)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            teacherId = 0;

            /*
                نحاول قراءة TeacherId من Session
                سواء كان محفوظًا كـ int أو string.
            */
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
            =====================================================
            تحويل اليوم الحالي إلى اسم عربي
            =====================================================

            نستخدمه حتى نقارن اليوم الحالي مع DayName
            الموجود في الجدول الدراسي.
        */
        private string GetArabicDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => "السبت",
                DayOfWeek.Sunday => "الأحد",
                DayOfWeek.Monday => "الاثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الأربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                _ => ""
            };
        }

        /*
            =====================================================
            تجهيز HttpClient مع التوكن
            =====================================================

            نستخدمه للاتصال بمشروع schoolApi.
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
            لوحة المدرس
            =====================================================

            هذه الصفحة تعرض:
            - اسم المدرس
            - رقم المدرس
            - عدد المواد
            - عدد الشعب
            - عدد التكليفات
            - عدد حصص اليوم
            - قائمة التكليفات
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            /*
                هذا الـ API موجود في AdminController داخل schoolApi.

                يرجع:
                - TeacherId
                - TeacherName
                - TeacherNumber
                - Assignments
            */
            var response = await client.GetAsync($"api/Admin/get-teacher-assignments-by-teacher/{teacherId}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل تكليفات المدرس.";

                return View(new TeacherDashboardViewModel
                {
                    TodayDayName = GetArabicDayName(DateTime.Now.DayOfWeek),
                    IsFridayHoliday = GetArabicDayName(DateTime.Now.DayOfWeek) == "الجمعة",
                    TodayClassesCount = 0
                });
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<TeacherDashboardViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new TeacherDashboardViewModel();

            /*
                =====================================================
                حساب عدد حصص اليوم للأستاذ
                =====================================================

                طريقة العمل:
                - نحدد اليوم الحالي بالعربي.
                - إذا كان اليوم الجمعة نعرض إجازة.
                - إذا لم يكن الجمعة، نجلب كل الجدول الدراسي.
                - نعد فقط الحصص المرتبطة بتكليفات الأستاذ الحالي.
                - إذا لا توجد حصص لهذا اليوم يظهر العدد 0.
            */
            var todayName = GetArabicDayName(DateTime.Now.DayOfWeek);

            model.TodayDayName = todayName;
            model.IsFridayHoliday = todayName == "الجمعة";
            model.TodayClassesCount = 0;

            if (!model.IsFridayHoliday && model.Assignments != null && model.Assignments.Any())
            {
                var teacherAssignmentIds = model.Assignments
                    .Select(a => a.Id)
                    .ToList();

                var schedulesResponse = await client.GetAsync("api/Admin/get-school-schedules");

                if (schedulesResponse.IsSuccessStatusCode)
                {
                    var schedulesJson = await schedulesResponse.Content.ReadAsStringAsync();

                    var schedules = JsonSerializer.Deserialize<List<TeacherScheduleViewModel>>(schedulesJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<TeacherScheduleViewModel>();

                    model.TodayClassesCount = schedules.Count(s =>
                        teacherAssignmentIds.Contains(s.TeacherAssignmentId) &&
                        s.DayName == todayName);
                }
            }

            return View(model);
        }
    }
}