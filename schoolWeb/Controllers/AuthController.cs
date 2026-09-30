using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        AuthController
        =====================================================

        هذا الكنترولر مسؤول عن:
        - عرض صفحة تسجيل الدخول
        - تنفيذ تسجيل الدخول
        - حفظ بيانات المستخدم في Session
        - توجيه المستخدم حسب الدور
        - تسجيل الخروج

        التعديل الجديد:
        نحفظ IsSystemAdmin في Session حتى نميز بين:
        - مدير النظام الرئيسي
        - مدير عادي

        مدير النظام الرئيسي فقط يستطيع إدارة المستخدمين.
    */
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            عرض صفحة تسجيل الدخول
            =====================================================
        */
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /*
            =====================================================
            تنفيذ تسجيل الدخول
            =====================================================

            طريقة العمل:
            1) نرسل اسم المستخدم وكلمة المرور إلى API.
            2) نستقبل بيانات تسجيل الدخول.
            3) نحفظ البيانات المهمة في Session.
            4) نوجه المستخدم حسب الدور.
        */
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("SchoolApi");

            var requestData = new
            {
                username = model.Username,
                password = model.Password
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
                return View(model);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var loginResult = JsonSerializer.Deserialize<LoginResponseViewModel>(responseString, options);

            if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تسجيل الدخول");
                return View(model);
            }

            /*
                =====================================================
                حفظ التوكن والبيانات في Session
                =====================================================

                هذه البيانات نستخدمها لاحقًا في Controllers للتحقق من:
                - هل المستخدم مسجل دخول؟
                - ما هو دوره؟
                - هل هو مدير النظام الرئيسي؟
            */
            HttpContext.Session.SetString("Token", loginResult.Token);
            HttpContext.Session.SetString("FullName", loginResult.FullName ?? "");
            HttpContext.Session.SetString("Username", loginResult.Username ?? "");
            HttpContext.Session.SetString("RoleName", loginResult.RoleName ?? "");
            HttpContext.Session.SetInt32("UserId", loginResult.UserId);

            /*
                هذا السطر مهم جدًا:
                إذا كان المستخدم مدير النظام الرئيسي نحفظ true.
                إذا كان Admin عادي أو Teacher أو Student نحفظ false.
            */
            HttpContext.Session.SetString(
                "IsSystemAdmin",
                loginResult.IsSystemAdmin ? "true" : "false"
            );

            /*
                حفظ StudentId إذا كان المستخدم طالبًا.
            */
            if (loginResult.StudentId.HasValue)
            {
                HttpContext.Session.SetInt32("StudentId", loginResult.StudentId.Value);
            }

            /*
                حفظ TeacherId إذا كان المستخدم مدرسًا.
            */
            if (loginResult.TeacherId.HasValue)
            {
                HttpContext.Session.SetInt32("TeacherId", loginResult.TeacherId.Value);
            }

            /*
                =====================================================
                التوجيه حسب الدور
                =====================================================
            */
            if (loginResult.RoleName == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            if (loginResult.RoleName == "Teacher")
            {
                return RedirectToAction("Index", "Teacher");
            }

            if (loginResult.RoleName == "Student")
            {
                return RedirectToAction("Index", "Student");
            }

            return RedirectToAction("Login");
        }

        /*
            =====================================================
            تسجيل الخروج
            =====================================================

            نحذف كل بيانات Session.
        */
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}