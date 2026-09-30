using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.Subject;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة المواد الدراسية في واجهة المدير

        من خلاله يستطيع المدير:
        - عرض المواد
        - إضافة مادة
        - تعديل مادة
        - حذف مادة
    */
    public class AdminSubjectController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminSubjectController(IHttpClientFactory httpClientFactory)
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
            1) عرض جميع المواد
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

            var response = await client.GetAsync("api/Admin/get-subjects");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل المواد.";
                return View(new List<SubjectViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var subjects = JsonSerializer.Deserialize<List<SubjectViewModel>>(json, options)
                           ?? new List<SubjectViewModel>();

            return View(subjects);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة مادة
            =====================================================
        */
        [HttpGet]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("Token");
            var roleName = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(token) || roleName != "Admin")
                return RedirectToAction("Login", "Auth");

            return View(new CreateSubjectViewModel());
        }

        /*
            =====================================================
            3) تنفيذ إضافة مادة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateSubjectViewModel model)
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

            var response = await client.PostAsync("api/Admin/create-subject", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", apiError);
                return View(model);
            }

            TempData["Success"] = "تم إضافة المادة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) فتح صفحة تعديل مادة
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

            var response = await client.GetAsync($"api/Admin/get-subject/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات المادة.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var subject = JsonSerializer.Deserialize<UpdateSubjectViewModel>(json, options);

            if (subject == null)
            {
                TempData["Error"] = "المادة غير موجودة.";
                return RedirectToAction("Index");
            }

            return View(subject);
        }

        /*
            =====================================================
            5) تنفيذ تعديل مادة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateSubjectViewModel model)
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

            var response = await client.PutAsync($"api/Admin/update-subject/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", apiError);
                return View(model);
            }

            TempData["Success"] = "تم تعديل المادة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) حذف مادة
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

            var response = await client.DeleteAsync($"api/Admin/delete-subject/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف المادة بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}