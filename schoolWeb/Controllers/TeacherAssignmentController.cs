using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.TeacherPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        TeacherAssignmentController
        =====================================================

        هذا الكنترولر مسؤول عن عرض تكليفات المدرس فقط.

        المدرس يرى:
        - المادة
        - الصف
        - الشعبة

        ولا يستطيع من هنا تعديل التكليفات؛ لأن التعديل من صلاحية المدير.
    */
    public class TeacherAssignmentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TeacherAssignmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من دخول المدرس
            =====================================================

            نحتاج:
            - Token
            - RoleName = Teacher
            - TeacherId

            TeacherId يجب أن يكون محفوظًا في Session عند تسجيل الدخول.
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

            return !string.IsNullOrEmpty(token)
                   && roleName == "Teacher"
                   && teacherId > 0;
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
            عرض تكليفات المدرس
            =====================================================

            هذه الصفحة تعرض للمدرس كل المواد والصفوف والشعب
            التي تم ربطه بها من قبل المدير.
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
                هذا الـ API موجود في schoolApi داخل AdminController

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
                return View(new TeacherAssignmentsPageViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<TeacherAssignmentsPageViewModel>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new TeacherAssignmentsPageViewModel();

            return View(model);
        }
    }
}