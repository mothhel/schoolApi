using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels.Lesson;
using schoolWeb.ViewModels.TeacherAssignment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة الدروس من واجهة المدير

        الدرس يرتبط بتكليف تدريسي
        والتكليف التدريسي يحتوي:
        - مدرس
        - مادة
        - صف
        - شعبة
    */
    public class AdminLessonController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminLessonController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            دالة مساعدة لإنشاء HttpClient مع التوكن
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
            دالة جلب التكليفات التدريسية للقائمة المنسدلة
            =====================================================

            هذه القائمة تظهر بهذا الشكل:
            أحمد محمد - رياضيات - الصف 1 - A
        */
        private async Task<List<SelectListItem>> GetTeacherAssignmentsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-teacher-assignments");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var assignments = JsonSerializer.Deserialize<List<TeacherAssignmentViewModel>>(json, options)
                              ?? new List<TeacherAssignmentViewModel>();

            return assignments
                .OrderBy(a => a.TeacherName)
                .ThenBy(a => a.SubjectName)
                .ThenBy(a => a.ClassRoomName)
                .ThenBy(a => a.SectionName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.TeacherName} - {a.SubjectName} - {a.ClassRoomName} - {a.SectionName}"
                })
                .ToList();
        }

        /*
            =====================================================
            1) عرض جميع الدروس
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-lessons");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الدروس.";
                return View(new List<LessonViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lessons = JsonSerializer.Deserialize<List<LessonViewModel>>(json, options)
                          ?? new List<LessonViewModel>();

            return View(lessons);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة درس
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var model = new CreateLessonViewModel
            {
                LessonDate = DateTime.Today,
                TeacherAssignments = await GetTeacherAssignmentsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            3) تنفيذ إضافة درس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateLessonViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherAssignmentId = model.TeacherAssignmentId,
                title = model.Title,
                description = model.Description,
                lessonDate = model.LessonDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-lesson", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم إضافة الدرس بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) فتح صفحة تعديل درس
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-lesson/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الدرس.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var lesson = JsonSerializer.Deserialize<UpdateLessonViewModel>(json, options);

            if (lesson == null)
            {
                TempData["Error"] = "الدرس غير موجود.";
                return RedirectToAction("Index");
            }

            lesson.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);

            return View(lesson);
        }

        /*
            =====================================================
            5) تنفيذ تعديل درس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateLessonViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherAssignmentId = model.TeacherAssignmentId,
                title = model.Title,
                description = model.Description,
                lessonDate = model.LessonDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-lesson/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الدرس بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) حذف درس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-lesson/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف الدرس بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}