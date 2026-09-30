using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using schoolWeb.ViewModels.Subject;
using schoolWeb.ViewModels.Teacher;
using schoolWeb.ViewModels.TeacherAssignment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة التكليفات التدريسية

        التكليف التدريسي يعني:
        - مدرس
        - مادة
        - صف
        - شعبة

        هذا الجزء مهم جدًا لأن الدروس والواجبات والدرجات والحضور
        تعتمد عليه لاحقًا
    */
    public class AdminTeacherAssignmentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminTeacherAssignmentController(IHttpClientFactory httpClientFactory)
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
            دالة جلب المدرسين للقائمة المنسدلة
            =====================================================
        */
        private async Task<List<SelectListItem>> GetTeachersForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-teachers");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var teachers = JsonSerializer.Deserialize<List<TeacherViewModel>>(json, options)
                           ?? new List<TeacherViewModel>();

            return teachers
                .OrderBy(t => t.FullName)
                .Select(t => new SelectListItem
                {
                    /*
                        نرسل رقم المدرس لأن API create-teacher-assignment
                        يبحث عن المدرس بواسطة TeacherNumber
                    */
                    Value = t.TeacherNumber,
                    Text = $"{t.FullName} - {t.TeacherNumber}"
                })
                .ToList();
        }

        /*
            =====================================================
            دالة جلب المواد للقائمة المنسدلة
            =====================================================
        */
        private async Task<List<SelectListItem>> GetSubjectsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-subjects");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var subjects = JsonSerializer.Deserialize<List<SubjectViewModel>>(json, options)
                           ?? new List<SubjectViewModel>();

            return subjects
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = s.Name
                })
                .ToList();
        }

        /*
            =====================================================
            دالة جلب الصفوف للقائمة المنسدلة
            =====================================================
        */
        private async Task<List<SelectListItem>> GetClassRoomsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json, options)
                             ?? new List<ClassRoomViewModel>();

            return classRooms
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                })
                .ToList();
        }

        /*
            =====================================================
            دالة جلب كل الشعب للقائمة المنسدلة
            =====================================================
        */
        private async Task<List<SelectListItem>> GetSectionsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var sections = JsonSerializer.Deserialize<List<SectionViewModel>>(json, options)
                           ?? new List<SectionViewModel>();

            return sections
                .OrderBy(s => s.ClassRoomName)
                .ThenBy(s => s.SectionName)
                .Select(s => new SelectListItem
                {
                    Value = s.SectionName,
                    Text = $"{s.SectionName} - {s.ClassRoomName}"
                })
                .ToList();
        }

        /*
            =====================================================
            API داخلي للـ MVC لجلب الشعب حسب الصف
            =====================================================

            هذا يستخدمه JavaScript في صفحة الإضافة والتعديل
        */
        [HttpGet]
        public async Task<IActionResult> GetSectionsByClassRoom(string classRoomName)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(classRoomName))
                return Json(new List<object>());

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
                return Json(new List<object>());

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var sections = JsonSerializer.Deserialize<List<SectionViewModel>>(json, options)
                           ?? new List<SectionViewModel>();

            var filteredSections = sections
                .Where(s => s.ClassRoomName.Trim().ToLower() == classRoomName.Trim().ToLower())
                .OrderBy(s => s.SectionName)
                .Select(s => new
                {
                    sectionName = s.SectionName
                })
                .ToList();

            return Json(filteredSections);
        }

        /*
            =====================================================
            1) عرض التكليفات التدريسية
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

            var response = await client.GetAsync("api/Admin/get-teacher-assignments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل التكليفات التدريسية.";
                return View(new List<TeacherAssignmentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var assignments = JsonSerializer.Deserialize<List<TeacherAssignmentViewModel>>(json, options)
                              ?? new List<TeacherAssignmentViewModel>();

            return View(assignments);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة تكليف تدريسي
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var model = new CreateTeacherAssignmentViewModel
            {
                Teachers = await GetTeachersForDropdown(token),
                Subjects = await GetSubjectsForDropdown(token),
                ClassRooms = await GetClassRoomsForDropdown(token),
                Sections = await GetSectionsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            3) تنفيذ إضافة تكليف تدريسي
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherAssignmentViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersForDropdown(token);
                model.Subjects = await GetSubjectsForDropdown(token);
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherNumber = model.TeacherNumber,
                subjectName = model.SubjectName,
                classRoomName = model.ClassRoomName,
                sectionName = model.SectionName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-teacher-assignment", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.Teachers = await GetTeachersForDropdown(token);
                model.Subjects = await GetSubjectsForDropdown(token);
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            TempData["Success"] = "تم إضافة التكليف التدريسي بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) فتح صفحة تعديل تكليف تدريسي
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

            var response = await client.GetAsync($"api/Admin/get-teacher-assignment/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات التكليف.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var assignment = JsonSerializer.Deserialize<UpdateTeacherAssignmentViewModel>(json, options);

            if (assignment == null)
            {
                TempData["Error"] = "التكليف غير موجود.";
                return RedirectToAction("Index");
            }

            assignment.Teachers = await GetTeachersForDropdown(token);
            assignment.Subjects = await GetSubjectsForDropdown(token);
            assignment.ClassRooms = await GetClassRoomsForDropdown(token);
            assignment.Sections = await GetSectionsForDropdown(token);

            return View(assignment);
        }

        /*
            =====================================================
            5) تنفيذ تعديل تكليف تدريسي
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherAssignmentViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersForDropdown(token);
                model.Subjects = await GetSubjectsForDropdown(token);
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherNumber = model.TeacherNumber,
                subjectName = model.SubjectName,
                classRoomName = model.ClassRoomName,
                sectionName = model.SectionName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-teacher-assignment/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.Teachers = await GetTeachersForDropdown(token);
                model.Subjects = await GetSubjectsForDropdown(token);
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            TempData["Success"] = "تم تعديل التكليف التدريسي بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) حذف تكليف تدريسي
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

            var response = await client.DeleteAsync($"api/Admin/delete-teacher-assignment/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف التكليف التدريسي بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}