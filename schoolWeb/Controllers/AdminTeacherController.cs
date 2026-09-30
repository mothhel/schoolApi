using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.Teacher;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة المدرسين من واجهة المدير
    */
    public class AdminTeacherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminTeacherController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            إنشاء HttpClient مع التوكن
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
            1) عرض جميع المدرسين
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

            var response = await client.GetAsync("api/Admin/get-teachers");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل المدرسين.";
                return View(new List<TeacherViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var teachers = JsonSerializer.Deserialize<List<TeacherViewModel>>(json, options)
                           ?? new List<TeacherViewModel>();

            return View(teachers);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة مدرس
            =====================================================
        */
        [HttpGet]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            return View(new CreateTeacherViewModel());
        }

        /*
            =====================================================
            3) تنفيذ إضافة مدرس
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherViewModel model)
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
                fullName = model.FullName,
                username = model.Username,
                password = model.Password,
                specialty = model.Specialty,
                phoneNumber = model.PhoneNumber,
                address = model.Address,
                hireDate = model.HireDate
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-teacher", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", apiError);
                return View(model);
            }

            TempData["Success"] = "تم إضافة المدرس بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) حذف مدرس
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

            var response = await client.DeleteAsync($"api/Admin/delete-teacher/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف المدرس بنجاح.";
            }

            return RedirectToAction("Index");
        }


        /*
    =====================================================
    5) فتح صفحة تعديل مدرس
    =====================================================

    هذه الميثود:
    - تستقبل Id المدرس
    - تجلب بيانات المدرس من الـ API
    - تعرضها في صفحة التعديل
*/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-teacher/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات المدرس.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var teacher = JsonSerializer.Deserialize<UpdateTeacherViewModel>(json, options);

            if (teacher == null)
            {
                TempData["Error"] = "المدرس غير موجود.";
                return RedirectToAction("Index");
            }

            return View(teacher);
        }

        /*
            =====================================================
            6) تنفيذ تعديل مدرس
            =====================================================

            هذه الميثود:
            - تستقبل البيانات المعدلة
            - ترسلها إلى API
            - إذا كانت كلمة المرور فارغة لا نغيرها
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateTeacherViewModel model)
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
                teacherNumber = model.TeacherNumber,
                fullName = model.FullName,
                username = model.Username,
                password = model.Password,
                specialty = model.Specialty,
                phoneNumber = model.PhoneNumber,
                address = model.Address,
                hireDate = model.HireDate,
                isActive = model.IsActive
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-teacher/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                return View(model);
            }

            TempData["Success"] = "تم تعديل المدرس بنجاح.";
            return RedirectToAction("Index");
        }
    }
}