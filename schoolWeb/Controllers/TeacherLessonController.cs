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
        TeacherLessonController
        =====================================================

        هذا الكنترولر مسؤول عن دروس المدرس فقط.

        المدرس يستطيع:
        - عرض دروسه
        - إضافة درس
        - تعديل درس
        - حذف درس

        مهم:
        المدرس لا يستطيع إضافة درس إلا على تكليف تدريسي خاص به.
    */
    public class TeacherLessonController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TeacherLessonController(IHttpClientFactory httpClientFactory)
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

            TeacherId لازم يكون محفوظًا في Session عند تسجيل الدخول.
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
            التي يسمح للمدرس بإضافة دروس لها.
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
            إذا حاول المدرس تعديل أو حذف درس ليس من تكليفاته
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
            عرض دروس المدرس
            =====================================================

            نأتي بكل الدروس من API
            ثم نعرض فقط الدروس المرتبطة بتكليفات المدرس الحالي.
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

            var response = await client.GetAsync("api/Admin/get-lessons");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل دروس المدرس.";
                return View(new List<TeacherLessonViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var lessons = JsonSerializer.Deserialize<List<TeacherLessonViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TeacherLessonViewModel>();

            /*
                نعرض فقط الدروس التي تخص تكليفات المدرس الحالي
            */
            lessons = lessons
                .Where(l => teacherAssignmentIds.Contains(l.TeacherAssignmentId))
                .OrderByDescending(l => l.LessonDate)
                .ToList();

            return View(lessons);
        }

        /*
            =====================================================
            فتح صفحة إضافة درس
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateTeacherLessonViewModel
            {
                LessonDate = DateTime.Today,
                TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId)
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ إضافة درس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherLessonViewModel model)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            /*
                حماية:
                لا يسمح للمدرس أن يضيف درسًا لتكليف ليس له
            */
            if (!teacherAssignmentIds.Contains(model.TeacherAssignmentId))
            {
                ModelState.AddModelError("", "لا يمكنك إضافة درس لهذا التكليف.");
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
                lessonDate = model.LessonDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-lesson", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
                return View(model);
            }

            TempData["Success"] = "تم إضافة الدرس بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            فتح صفحة تعديل درس
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

            var response = await client.GetAsync($"api/Admin/get-lesson/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الدرس.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var lesson = JsonSerializer.Deserialize<UpdateTeacherLessonViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (lesson == null)
            {
                TempData["Error"] = "الدرس غير موجود.";
                return RedirectToAction("Index");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            /*
                حماية:
                إذا كان الدرس ليس من تكليفات المدرس الحالي نمنعه من تعديله.
            */
            if (!teacherAssignmentIds.Contains(lesson.TeacherAssignmentId))
            {
                TempData["Error"] = "لا يمكنك تعديل هذا الدرس.";
                return RedirectToAction("Index");
            }

            lesson.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);

            return View(lesson);
        }

        /*
            =====================================================
            تنفيذ تعديل درس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherLessonViewModel model)
        {
            if (!IsTeacherLoggedIn(out string token, out int teacherId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            if (!teacherAssignmentIds.Contains(model.TeacherAssignmentId))
            {
                ModelState.AddModelError("", "لا يمكنك تعديل درس لهذا التكليف.");
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
                lessonDate = model.LessonDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-lesson/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.TeacherAssignments = await GetTeacherAssignmentsForDropdown(token, teacherId);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الدرس بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            حذف درس
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
                قبل الحذف نتحقق أن الدرس يخص المدرس الحالي
            */
            var lessonResponse = await client.GetAsync($"api/Admin/get-lesson/{id}");

            if (!lessonResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الدرس.";
                return RedirectToAction("Index");
            }

            var lessonJson = await lessonResponse.Content.ReadAsStringAsync();

            var lesson = JsonSerializer.Deserialize<TeacherLessonViewModel>(lessonJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (lesson == null)
            {
                TempData["Error"] = "الدرس غير موجود.";
                return RedirectToAction("Index");
            }

            var teacherAssignmentIds = await GetTeacherAssignmentIds(token, teacherId);

            if (!teacherAssignmentIds.Contains(lesson.TeacherAssignmentId))
            {
                TempData["Error"] = "لا يمكنك حذف هذا الدرس.";
                return RedirectToAction("Index");
            }

            var response = await client.DeleteAsync($"api/Admin/delete-lesson/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "تم حذف الدرس بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}