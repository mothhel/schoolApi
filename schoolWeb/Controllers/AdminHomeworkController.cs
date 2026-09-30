using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels.Homework;
using schoolWeb.ViewModels.TeacherAssignment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة الواجبات من واجهة المدير

        الواجب يرتبط بتكليف تدريسي
        والتكليف التدريسي يحتوي:
        - مدرس
        - مادة
        - صف
        - شعبة
    */
    public class AdminHomeworkController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminHomeworkController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /*
            جلب التكليفات التدريسية للقائمة المنسدلة
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
            1) عرض جميع الواجبات
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

            var response = await client.GetAsync("api/Admin/get-homeworks");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الواجبات.";
                return View(new List<HomeworkViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var homeworks = JsonSerializer.Deserialize<List<HomeworkViewModel>>(json, options)
                            ?? new List<HomeworkViewModel>();

            return View(homeworks);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة واجب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var model = new CreateHomeworkViewModel
            {
                DueDate = DateTime.Today,
                TeacherAssignments = await GetTeacherAssignmentsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            3) تنفيذ إضافة واجب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateHomeworkViewModel model)
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
                dueDate = model.DueDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-homework", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم إضافة الواجب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) فتح صفحة تعديل واجب
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

            var response = await client.GetAsync($"api/Admin/get-homework/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الواجب.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var homework = JsonSerializer.Deserialize<UpdateHomeworkViewModel>(json, options);

            if (homework == null)
            {
                TempData["Error"] = "الواجب غير موجود.";
                return RedirectToAction("Index");
            }

            homework.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);

            return View(homework);
        }

        /*
            =====================================================
            5) تنفيذ تعديل واجب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateHomeworkViewModel model)
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
                dueDate = model.DueDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-homework/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الواجب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) حذف واجب
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

            var response = await client.DeleteAsync($"api/Admin/delete-homework/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف الواجب بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}