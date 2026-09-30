using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels.UserManagement;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        كنترولر إدارة المستخدمين
        =====================================================

        هذا الكنترولر مسؤول عن:
        - عرض المستخدمين
        - فلترة المستخدمين حسب النوع
        - تفعيل وتعطيل الحساب
        - تعديل اسم المستخدم
        - تغيير كلمة المرور
        - حذف المستخدم

        مهم:
        هذا الكنترولر مسموح فقط لمدير النظام الرئيسي.
        أي Admin عادي لا يستطيع الدخول إليه.
    */
    public class AdminUserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminUserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من أن المستخدم الحالي مدير النظام الرئيسي
            =====================================================

            الشروط:
            - يوجد Token
            - RoleName = Admin
            - IsSystemAdmin = true

            إذا كان المستخدم Admin عادي:
            لا يسمح له بالدخول إلى إدارة المستخدمين.
        */
        private bool IsSystemAdminLoggedIn(out string token)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");
            var isSystemAdmin = HttpContext.Session.GetString("IsSystemAdmin");

            return !string.IsNullOrWhiteSpace(token)
                   && roleName == "Admin"
                   && isSystemAdmin == "true";
        }

        /*
            =====================================================
            إنشاء HttpClient مع التوكن
            =====================================================
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
            تجهيز خيارات فلترة المستخدمين
            =====================================================
        */
        private List<SelectListItem> GetRoleOptions(string selectedRole)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "All",
                    Text = "عرض جميع المستخدمين",
                    Selected = selectedRole == "All"
                },
                new SelectListItem
                {
                    Value = "Admin",
                    Text = "المدير",
                    Selected = selectedRole == "Admin"
                },
                new SelectListItem
                {
                    Value = "Teacher",
                    Text = "المدرسون",
                    Selected = selectedRole == "Teacher"
                },
                new SelectListItem
                {
                    Value = "Student",
                    Text = "الطلاب",
                    Selected = selectedRole == "Student"
                }
            };
        }

        /*
            =====================================================
            عرض المستخدمين
            =====================================================

            roleFilter:
            - All
            - Admin
            - Teacher
            - Student
        */
        [HttpGet]
        public async Task<IActionResult> Index(string roleFilter = "All")
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-users?roleName={roleFilter}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل المستخدمين.";
                ViewBag.RoleOptions = GetRoleOptions(roleFilter);
                ViewBag.SelectedRole = roleFilter;

                return View(new List<UserViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<UserViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<UserViewModel>();

            ViewBag.RoleOptions = GetRoleOptions(roleFilter);
            ViewBag.SelectedRole = roleFilter;

            return View(users);
        }

        /*
            =====================================================
            فتح صفحة تعديل اسم المستخدم
            =====================================================

            مسموح فقط لمدير النظام الرئيسي.
        */
        [HttpGet]
        public async Task<IActionResult> EditUsername(int id)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            var client = CreateClientWithToken(token);

            /*
                نجلب بيانات المستخدم الحالية حتى نعرض:
                - الاسم الكامل
                - اسم المستخدم الحالي
                - نوع الحساب
            */
            var response = await client.GetAsync($"api/Auth/get-user/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات المستخدم.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود.";
                return RedirectToAction("Index");
            }

            var model = new EditUsernameViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                CurrentUsername = user.Username,
                NewUsername = user.Username,
                RoleName = user.RoleName
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ تعديل اسم المستخدم
            =====================================================

            مسموح فقط لمدير النظام الرئيسي.

            يستخدم API:
            api/Admin/update-username/{userId}
        */
        [HttpPost]
        public async Task<IActionResult> EditUsername(EditUsernameViewModel model)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.NewUsername))
            {
                ModelState.AddModelError("NewUsername", "اسم المستخدم الجديد مطلوب.");
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                username = model.NewUsername.Trim()
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(
                $"api/Admin/update-username/{model.UserId}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "تعذر تعديل اسم المستخدم.";
                }

                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "تم تعديل اسم المستخدم بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            تفعيل أو تعطيل حساب المستخدم
            =====================================================

            نستقبل:
            - id المستخدم
            - الحالة الجديدة

            مسموح فقط لمدير النظام الرئيسي.
        */
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                isActive = isActive
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-user-status/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = isActive
                    ? "تم تفعيل الحساب بنجاح."
                    : "تم تعطيل الحساب بنجاح.";
            }

            return RedirectToAction("Index");
        }

        /*
            =====================================================
            فتح صفحة تغيير كلمة المرور
            =====================================================

            مسموح فقط لمدير النظام الرئيسي.
        */
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Auth/get-user/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات المستخدم.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود.";
                return RedirectToAction("Index");
            }

            var model = new ChangeUserPasswordViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Username = user.Username
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ تغيير كلمة المرور
            =====================================================

            نستخدم endpoint الموجود في AuthController:
            api/Auth/admin-reset-password/{userId}

            مسموح فقط لمدير النظام الرئيسي.
        */
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangeUserPasswordViewModel model)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateClientWithToken(token);

            /*
                الـ API يستقبل كلمة المرور الجديدة كنص JSON.
                لذلك نرسل JsonSerializer.Serialize(model.NewPassword)
            */
            var json = JsonSerializer.Serialize(model.NewPassword);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"api/Auth/admin-reset-password/{model.UserId}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "تم تغيير كلمة المرور بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            حذف المستخدم
            =====================================================

            مسموح فقط لمدير النظام الرئيسي.
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsSystemAdminLoggedIn(out string token))
            {
                return RedirectToAction("Index", "Admin");
            }

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-user/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "تم حذف المستخدم بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}