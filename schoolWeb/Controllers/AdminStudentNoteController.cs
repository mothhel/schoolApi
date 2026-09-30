using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using schoolWeb.ViewModels.StudentNote;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        هذا الكنترولر مسؤول عن إدارة ملاحظات الطلاب:
        - عرض الملاحظات
        - إضافة ملاحظة
        - تعديل ملاحظة
        - حذف ملاحظة
    */
    public class AdminStudentNoteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminStudentNoteController(IHttpClientFactory httpClientFactory)
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

        /*
            هذه الدالة تجلب الطلاب لاستخدامهم في قائمة الاختيار
        */
        private async Task<List<SelectListItem>> GetStudentsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-students");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var students = JsonSerializer.Deserialize<List<StudentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<StudentViewModel>();

            return students
                .OrderBy(s => s.FullName)
                .Select(s => new SelectListItem
                {
                    Value = s.StudentNumber,
                    Text = $"{s.FullName} - {s.StudentNumber} - {s.ClassRoomName} - {s.SectionName}"
                })
                .ToList();
        }

        /*
            عرض جميع ملاحظات الطلاب
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-student-notes");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل ملاحظات الطلاب.";
                return View(new List<StudentNoteViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var notes = JsonSerializer.Deserialize<List<StudentNoteViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<StudentNoteViewModel>();

            return View(notes);
        }

        /*
            فتح صفحة إضافة ملاحظة جديدة
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var model = new CreateStudentNoteViewModel
            {
                Students = await GetStudentsForDropdown(token)
            };

            return View(model);
        }

        /*
            تنفيذ إضافة ملاحظة جديدة
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentNoteViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = model.StudentNumber,
                noteText = model.NoteText
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-student-note", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم إضافة الملاحظة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            فتح صفحة تعديل ملاحظة
        */
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student-note/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الملاحظة.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var note = JsonSerializer.Deserialize<UpdateStudentNoteViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (note == null)
            {
                TempData["Error"] = "الملاحظة غير موجودة.";
                return RedirectToAction("Index");
            }

            note.Students = await GetStudentsForDropdown(token);

            return View(note);
        }

        /*
            تنفيذ تعديل الملاحظة
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateStudentNoteViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = model.StudentNumber,
                noteText = model.NoteText
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-student-note/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);

                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الملاحظة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            حذف ملاحظة
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-student-note/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "تم حذف الملاحظة بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}