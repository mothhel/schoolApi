using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    public class AdminStudentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminStudentController(IHttpClientFactory httpClientFactory)
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
            =====================================================
            دالة جديدة 🔥 جلب الشعب حسب الصف
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> GetSectionsByClassRoom(string classRoomName)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return Unauthorized();

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

            var filtered = sections
                .Where(s => s.ClassRoomName.Trim().ToLower() == classRoomName.Trim().ToLower())
                .OrderBy(s => s.SectionName)
                .Select(s => new
                {
                    sectionName = s.SectionName
                })
                .ToList();

            return Json(filtered);
        }

        /*
            =====================================================
            دالة الصفوف Dropdown
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
            دالة الشعب Dropdown (كل الشعب)
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
            عرض الطلاب
            =====================================================
        */
        /*
    =====================================================
    1) عرض جميع الطلاب + البحث
    =====================================================

    هذه الصفحة:
    - تعرض الطلاب
    - تسمح بالبحث بالاسم أو رقم الطالب أو اسم المستخدم
*/
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-students");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الطلاب.";
                return View(new List<StudentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var students = JsonSerializer.Deserialize<List<StudentViewModel>>(json, options)
                           ?? new List<StudentViewModel>();

            /*
                إذا كتب المدير كلمة بحث
                نفلتر الطلاب حسب:
                - الاسم
                - رقم الطالب
                - اسم المستخدم
            */
            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students
                    .Where(s =>
                        s.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        s.StudentNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        s.Username.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Search = search;

            return View(students);
        }

        /*
            =====================================================
            فتح صفحة إضافة طالب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var model = new CreateStudentViewModel
            {
                ClassRooms = await GetClassRoomsForDropdown(token),
                Sections = await GetSectionsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ إضافة طالب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                fullName = model.FullName,
                username = model.Username,
                password = model.Password,
                //studentNumber = model.StudentNumber,
                gender = model.Gender,
                birthDate = model.BirthDate,
                address = model.Address,
                phoneNumber = model.PhoneNumber,
                parentName = model.ParentName,
                parentPhone = model.ParentPhone,
                classRoomName = model.ClassRoomName,
                sectionName = model.SectionName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-student", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            TempData["Success"] = "تم إضافة الطالب بنجاح.";
            return RedirectToAction("Index");
        }


        /*
    =====================================================
    4) حذف طالب
    =====================================================

    هذه الميثود:
    - تستقبل Id الطالب
    - ترسل طلب حذف إلى API
    - ثم تعود إلى صفحة الطلاب
*/
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-student/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف الطالب بنجاح.";
            }

            return RedirectToAction("Index");
        }



        /*
    =====================================================
    5) فتح صفحة تعديل طالب
    =====================================================

    هذه الميثود:
    - تستقبل Id الطالب
    - تجلب بيانات الطالب من API
    - تجهز قوائم الصفوف والشعب
    - تعرض صفحة التعديل
*/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الطالب.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var student = JsonSerializer.Deserialize<UpdateStudentViewModel>(json, options);

            if (student == null)
            {
                TempData["Error"] = "الطالب غير موجود.";
                return RedirectToAction("Index");
            }

            /*
                تحميل الصفوف والشعب للـ Dropdown
            */
            student.ClassRooms = await GetClassRoomsForDropdown(token);
            student.Sections = await GetSectionsForDropdown(token);

            return View(student);
        }

        /*
            =====================================================
            6) تنفيذ تعديل طالب
            =====================================================

            هذه الميثود:
            - تستقبل بيانات الطالب المعدلة
            - ترسلها إلى API
            - ثم تعيد المستخدم إلى صفحة الطلاب
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateStudentViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            /*
                مهم:
                API تعديل الطالب ما زال يحتاج StudentNumber
                لذلك نرسله كما هو بدون أن يكتبه المستخدم
            */
            var requestData = new
            {
                studentNumber = model.StudentNumber,
                fullName = model.FullName,
                username = model.Username,
                password = model.Password,
                gender = model.Gender,
                birthDate = model.BirthDate,
                address = model.Address,
                phoneNumber = model.PhoneNumber,
                parentName = model.ParentName,
                parentPhone = model.ParentPhone,
                classRoomName = model.ClassRoomName,
                sectionName = model.SectionName,
                isActive = model.IsActive
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-student/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.ClassRooms = await GetClassRoomsForDropdown(token);
                model.Sections = await GetSectionsForDropdown(token);

                return View(model);
            }

            TempData["Success"] = "تم تعديل الطالب بنجاح.";
            return RedirectToAction("Index");
        }
    }
}