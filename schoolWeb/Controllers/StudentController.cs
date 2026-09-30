using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentController
        =====================================================

        هذا الكنترولر مسؤول عن لوحة الطالب الرئيسية.

        يعرض:
        - بيانات الطالب الأساسية
        - بياناته الشخصية
        - روابط الطالب
        - عدادات الجديد فقط

        ملاحظة مهمة:
        لم نعد نحسب العدد الكلي للدروس أو الواجبات أو الملاحظات هنا.
        الآن نعتمد على API:

        api/Student/my-new-counts

        هذا API يرجع عدد الأشياء الجديدة فقط.
        وإذا كان العدد 0، الواجهة تخفي العداد.
    */
    public class StudentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من تسجيل دخول الطالب
            =====================================================

            نحتاج:
            - Token
            - RoleName = Student
            - StudentId

            StudentId يستخدم هنا فقط لجلب بيانات الطالب الأساسية
            من API الموجود عندك:
            api/Admin/get-student/{id}
        */
        private bool IsStudentLoggedIn(out string token, out int studentId)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            studentId = 0;

            /*
                نحاول قراءة StudentId من Session
                سواء كان محفوظًا كـ int أو string.
            */
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
            تصفير عدادات الجديد في حالة فشل تحميلها
            =====================================================

            حتى لا تظهر أرقام قديمة أو غير صحيحة.
        */
        private void ResetNewCounts(StudentDashboardViewModel model)
        {
            model.LessonsCount = 0;
            model.HomeworksCount = 0;
            model.AttendanceCount = 0;
            model.ScheduleCount = 0;
            model.NotesCount = 0;
        }

        /*
            =====================================================
            تحميل عدادات الجديد للطالب الحالي
            =====================================================

            هذا يعتمد على التوكن.
            لا نرسل StudentId من الواجهة.

            API المستخدم:
            api/Student/my-new-counts
        */
        private async Task LoadStudentNewCounts(HttpClient client, StudentDashboardViewModel model)
        {
            ResetNewCounts(model);

            try
            {
                var countsResponse = await client.GetAsync("api/Student/my-new-counts");

                if (!countsResponse.IsSuccessStatusCode)
                {
                    return;
                }

                var countsJson = await countsResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(countsJson))
                {
                    return;
                }

                var counts = JsonSerializer.Deserialize<StudentPortalNewCountsViewModel>(
                    countsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (counts == null)
                {
                    return;
                }

                model.LessonsCount = counts.LessonsCount;
                model.HomeworksCount = counts.HomeworksCount;
                model.AttendanceCount = counts.AttendanceCount;
                model.ScheduleCount = counts.ScheduleCount;
                model.NotesCount = counts.NotesCount;
            }
            catch
            {
                /*
                    إذا حدث خطأ في تحميل العدادات،
                    لا نوقف لوحة الطالب.
                    فقط نخلي العدادات 0.
                */
                ResetNewCounts(model);
            }
        }

        /*
            =====================================================
            لوحة الطالب الرئيسية
            =====================================================

            طريقة العمل:
            1) نقرأ StudentId من Session.
            2) نجلب بيانات الطالب من API.
            3) نجلب عدادات الجديد من API الطالب.
            4) نرسل البيانات إلى View.

            ملاحظة:
            عند فتح الطالب صفحة دروسي أو واجباتي أو ملاحظاتي...
            يتم تصفير عداد ذلك القسم من Controller الخاص بتلك الصفحة.
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsStudentLoggedIn(out string token, out int studentId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            /*
                =====================================================
                جلب بيانات الطالب الحالي
                =====================================================

                نستخدم API الموجود عندك:
                api/Admin/get-student/{id}
            */
            var response = await client.GetAsync($"api/Admin/get-student/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل بيانات الطالب.";

                var emptyModel = new StudentDashboardViewModel();
                ResetNewCounts(emptyModel);

                return View(emptyModel);
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<StudentDashboardViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new StudentDashboardViewModel();

            /*
                =====================================================
                جلب عدادات الجديد
                =====================================================

                هذه العدادات تعني:
                - دروس جديدة لم يشاهدها الطالب
                - واجبات جديدة لم يشاهدها الطالب
                - حضور وغياب جديد لم يشاهده الطالب
                - جدول جديد لم يشاهده الطالب
                - ملاحظات جديدة لم يشاهدها الطالب
            */
            await LoadStudentNewCounts(client, model);

            return View(model);
        }
    }
}