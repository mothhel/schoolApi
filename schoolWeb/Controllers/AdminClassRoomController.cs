using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة الصفوف والشعب في الواجهة

        بعد التحسين أصبح يدعم:
        - عرض الصفوف
        - إضافة صف
        - تعديل صف
        - حذف صف
        - عرض الشعب
        - إضافة شعبة
        - تعديل شعبة
        - حذف شعبة

        جميع العمليات تتم من خلال الاتصال بمشروع الـ API
    */
    public class AdminClassRoomController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminClassRoomController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            دالة مساعدة لإنشاء HttpClient مع التوكن
            =====================================================

            هذه الدالة تقلل التكرار داخل الكنترولر
            حيث:
            - تنشئ HttpClient
            - تضيف Bearer Token
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
            دالة مساعدة لجلب الصفوف وتحويلها إلى SelectListItem
            =====================================================

            نستخدمها في:
            - صفحة إضافة شعبة
            - صفحة تعديل شعبة
        */
        private async Task<List<SelectListItem>> GetClassRoomsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json, options)
                             ?? new List<ClassRoomViewModel>();

            return classRooms.Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            }).ToList();
        }

        /*
            =====================================================
            1) عرض جميع الصفوف
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

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الصفوف.";
                return View(new List<ClassRoomViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json, options)
                 ?? new List<ClassRoomViewModel>();

            /*
                هنا نقوم بترتيب الصفوف حسب الرقم الموجود داخل اسم الصف
                مثال:
                الصف 1
                الصف 2
                الصف 10

                بدل أن تكون مرتبة حسب Id قاعدة البيانات
            */
            classRooms = classRooms
                .OrderBy(c =>
                {
                    // استخراج الأرقام فقط من اسم الصف
                    var digits = new string(c.Name.Where(char.IsDigit).ToArray());

                    // إذا وجدنا رقمًا نرتب به
                    if (int.TryParse(digits, out int classNumber))
                        return classNumber;

                    // إذا لم يوجد رقم نضعه في آخر القائمة
                    return int.MaxValue;
                })
                .ThenBy(c => c.Name)
                .ToList();

            return View(classRooms);
        }

        /*
            =====================================================
            2) إضافة صف جديد
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> CreateClassRoom(CreateClassRoomViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "البيانات المدخلة غير صحيحة.";
                return RedirectToAction("Index");
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                name = model.Name
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-classroom", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل إضافة الصف.";
            }
            else
            {
                TempData["Success"] = "تم إضافة الصف بنجاح.";
            }

            return RedirectToAction("Index");
        }

        /*
            =====================================================
            3) فتح صفحة تعديل صف
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> EditClassRoom(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الصف.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json, options)
                             ?? new List<ClassRoomViewModel>();

            var classRoom = classRooms.FirstOrDefault(c => c.Id == id);

            if (classRoom == null)
            {
                TempData["Error"] = "الصف غير موجود.";
                return RedirectToAction("Index");
            }

            var model = new UpdateClassRoomViewModel
            {
                Id = classRoom.Id,
                Name = classRoom.Name
            };

            return View(model);
        }

        /*
            =====================================================
            4) تنفيذ تعديل الصف
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> EditClassRoom(UpdateClassRoomViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                name = model.Name
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-classroom/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل تعديل الصف.";
                return View(model);
            }

            TempData["Success"] = "تم تعديل الصف بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            5) حذف صف
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> DeleteClassRoom(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-classroom/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل حذف الصف. قد يكون مرتبطًا بشعب أو طلاب.";
            }
            else
            {
                TempData["Success"] = "تم حذف الصف بنجاح.";
            }

            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) عرض جميع الشعب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Sections()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الشعب.";
                return View(new CreateSectionPageViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var sections = JsonSerializer.Deserialize<List<SectionViewModel>>(json, options)
               ?? new List<SectionViewModel>();

            /*
                هنا نرتب الشعب كالتالي:
                1) حسب رقم الصف
                2) ثم حسب اسم الشعبة

                مثال:
                الصف 1 - A
                الصف 1 - B
                الصف 2 - A
                الصف 2 - B
            */
            sections = sections
                .OrderBy(s =>
                {
                    // استخراج الرقم من اسم الصف
                    var digits = new string(s.ClassRoomName.Where(char.IsDigit).ToArray());

                    if (int.TryParse(digits, out int classNumber))
                        return classNumber;

                    return int.MaxValue;
                })
                .ThenBy(s => s.SectionName)
                .ToList();

            var pageModel = new CreateSectionPageViewModel
            {
                Sections = sections,
                CreateSection = new CreateSectionViewModel
                {
                    ClassRooms = await GetClassRoomsForDropdown(token)
                }
            };

            return View(pageModel);
        }

        /*
            =====================================================
            7) إضافة شعبة جديدة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> CreateSection(CreateSectionViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "البيانات المدخلة غير صحيحة.";
                return RedirectToAction("Sections");
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                name = model.Name,
                classRoomName = model.ClassRoomName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-section", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل إضافة الشعبة.";
            }
            else
            {
                TempData["Success"] = "تم إضافة الشعبة بنجاح.";
            }

            return RedirectToAction("Sections");
        }

        /*
            =====================================================
            8) فتح صفحة تعديل شعبة
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> EditSection(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الشعبة.";
                return RedirectToAction("Sections");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var sections = JsonSerializer.Deserialize<List<SectionViewModel>>(json, options)
                           ?? new List<SectionViewModel>();

            var section = sections.FirstOrDefault(s => s.Id == id);

            if (section == null)
            {
                TempData["Error"] = "الشعبة غير موجودة.";
                return RedirectToAction("Sections");
            }

            var model = new UpdateSectionViewModel
            {
                Id = section.Id,
                Name = section.SectionName,
                ClassRoomName = section.ClassRoomName,
                ClassRooms = await GetClassRoomsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            9) تنفيذ تعديل الشعبة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> EditSection(UpdateSectionViewModel model)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                name = model.Name,
                classRoomName = model.ClassRoomName
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-section/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل تعديل الشعبة.";
                model.ClassRooms = await GetClassRoomsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الشعبة بنجاح.";
            return RedirectToAction("Sections");
        }

        /*
            =====================================================
            10) حذف شعبة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-section/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل حذف الشعبة. قد تكون مرتبطة بطلاب.";
            }
            else
            {
                TempData["Success"] = "تم حذف الشعبة بنجاح.";
            }

            return RedirectToAction("Sections");
        }
    }

    /*
        هذا الكلاس يمثل نموذج الصفحة الخاصة بالشعب

        لأن صفحة الشعب أصبحت تحتوي على:
        - نموذج إضافة شعبة
        - جدول عرض جميع الشعب

        لذلك جمعناهما في ViewModel واحد
    */
    public class CreateSectionPageViewModel
    {
        public CreateSectionViewModel CreateSection { get; set; } = new();
        public List<SectionViewModel> Sections { get; set; } = new();
    }
}