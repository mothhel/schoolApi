using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.Admin;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsAdminLoggedIn(out string token)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            return !string.IsNullOrEmpty(token) && roleName == "Admin";
        }

        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/dashboard-stats");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل إحصائيات لوحة المدير.";
                return View(new DashboardViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<DashboardViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new DashboardViewModel();

            return View(model);
        }
    }
}