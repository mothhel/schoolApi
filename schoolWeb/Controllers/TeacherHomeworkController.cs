using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels.TeacherPortal;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        TeacherHomeworkController
        =====================================================

        هذا الكنترولر مسؤول عن واجبات المدرس فقط.

        المدرس يستطيع:
        - عرض واجباته
        - إضافة واجب
        - تعديل واجب
        - حذف واجب

        مهم:
        المدرس لا يستطيع إضافة واجب إلا على تكليف تدريسي خاص به.
    */
    public class TeacherHomeworkController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TeacherHomeworkController(IHttpClientFactory httpClientFactory)
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
            =====================================================
            جلب تكليفات المدرس
            =====================================================

            نستخدمها حتى نعرف ما هي المواد والصفوف والشعب
            التي يسمح للمدرس بإضافة واجبات لها.
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
            تجهيز التكليفات في Dropdown
            =====================================================

            تظهر للمدرس بهذا الشكل:
            رياضيات - الصف الأول - A
        */
        private async Task<List<SelectListItem>> GetTeacherAssignmentsForDropdown(string token, int teacherId)
        {
            var page = await GetTeacherAssignmentsPage(token, teacherId);

            return page.Assignments
                .OrderBy(a => a.SubjectName)
                .ThenBy(a => a.ClassRoomName)
                .ThenBy(a => a.SectionName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.SubjectName} - {a.ClassRoomName} - {a.SectionName}"
                })
                .ToList();
        }

        /*
            =====================================================
            جلب أرقام التكليفات الخاصة بالمدرس
            =====================================================

            نستخدمها للحماية:
            إذا حاول المدرس تعديل أو حذف واجب ليس من تكليفاته
            نمنعه.
        */
        private async Task<List<int>> GetTeacherAssignmentIds(string token, int teacherId)
        {
            var page = await GetTeacherAssignmentsPage(token, teacherId);

            return page.Assignments
                .Select(a => a.Id)
                .ToList();
        }

        /*
            =====================================================
            عرض واجبات المدرس
            =====================================================

            نأتي بكل الواجبات من API
            ثم نعرض فقط الواجبات المرتبطة بتكليفات المدرس الحالي.
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            var response = await client.GetAsync("api/Admin/get-homeworks");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل واجبات المدرس.";
                return View(new List<TeacherHomeworkViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var homeworks = JsonSerializer.Deserialize<List<TeacherHomeworkViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TeacherHomeworkViewModel>();

            /*
                نعرض فقط الواجبات التي تخص تكليفات المدرس الحالي
            */
            homeworks = homeworks
                .Where(h => teacherAssignmentIds.Contains(h.TeacherAssignmentId))
                .OrderByDescending(h => h.DueDate)
                .ToList();

            return View(homeworks);
        }

        /*
            =====================================================
            فتح صفحة إضافة واجب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateTeacherHomeworkViewModel
            {
                DueDate = DateTime.Today,
                TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId)
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ إضافة واجب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherHomeworkViewModel model)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            /*
                حماية:
                لا يسمح للمدرس أن يضيف واجبًا لتكليف ليس له.
            */
            if (!teacherAssignmentIds.Contains(model.TeacherAssignmentId))
            {
                ModelState.AddModelError("", "لا يمكنك إضافة واجب لهذا التكليف.");
            }

            if (!ModelState.IsValid)
            {
                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
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
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
                return View(model);
            }

            TempData["Success"] = "تم إضافة الواجب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            فتح صفحة تعديل واجب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-homework/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الواجب.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var homework = JsonSerializer.Deserialize<UpdateTeacherHomeworkViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (homework == null)
            {
                TempData["Error"] = "الواجب غير موجود.";
                return RedirectToAction("Index");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            /*
                حماية:
                إذا كان الواجب ليس من تكليفات المدرس الحالي نمنعه من تعديله.
            */
            if (!teacherAssignmentIds.Contains(homework.TeacherAssignmentId))
            {
                TempData["Error"] = "لا يمكنك تعديل هذا الواجب.";
                return RedirectToAction("Index");
            }

            homework.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);

            return View(homework);
        }

        /*
            =====================================================
            تنفيذ تعديل واجب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherHomeworkViewModel model)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            if (!teacherAssignmentIds.Contains(model.TeacherAssignmentId))
            {
                ModelState.AddModelError("", "لا يمكنك تعديل واجب لهذا التكليف.");
            }

            if (!ModelState.IsValid)
            {
                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
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
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الواجب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            حذف واجب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            /*
                قبل الحذف نتحقق أن الواجب يخص المدرس الحالي.
            */
            var homeworkResponse = await client.GetAsync($"api/Admin/get-homework/{id}");

            if (!homeworkResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الواجب.";
                return RedirectToAction("Index");
            }

            var homeworkJson = await homeworkResponse.Content.ReadAsStringAsync();

            var homework = JsonSerializer.Deserialize<TeacherHomeworkViewModel>(homeworkJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (homework == null)
            {
                TempData["Error"] = "الواجب غير موجود.";
                return RedirectToAction("Index");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            if (!teacherAssignmentIds.Contains(homework.TeacherAssignmentId))
            {
                TempData["Error"] = "لا يمكنك حذف هذا الواجب.";
                return RedirectToAction("Index");
            }

            var response = await client.DeleteAsync($"api/Admin/delete-homework/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "تم حذف الواجب بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}