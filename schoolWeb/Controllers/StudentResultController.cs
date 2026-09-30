using Microsoft.AspNetCore.Mvc;
using schoolWeb.ViewModels.Grade;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentResultController
        =====================================================

        هذا الكنترولر مسؤول عن عرض نتيجة الطالب الحالي.

        الطالب لا يختار طالبًا آخر.
        يتم معرفة الطالب من Session عن طريق StudentId.
    */
    public class StudentResultController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentResultController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            التحقق من دخول الطالب
            =====================================================

            نحتاج:
            - Token
            - RoleName = Student
            - StudentId
        */
        private bool IsStudentLoggedIn(out string token, out int studentId)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            studentId = 0;

            var studentIdFromInt = HttpContext.Session.GetInt32("StudentId");

            if (studentIdFromInt.HasValue)
            {
                studentId = studentIdFromInt.Value;
            }
            else
            {
                var studentIdText = HttpContext.Session.GetString("StudentId");
                int.TryParse(studentIdText, out studentId);
            }

            return !string.IsNullOrWhiteSpace(token)
                   && roleName == "Student"
                   && studentId > 0;
        }

        /*
            =====================================================
            تجهيز HttpClient مع التوكن
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
            جلب بيانات الطالب الحالي
            =====================================================

            نحتاج رقم الطالب StudentNumber
            لأن API النتيجة يعتمد على رقم الطالب وليس Id.
        */
        private async Task<StudentDashboardViewModel?> GetCurrentStudent(string token, int studentId)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StudentDashboardViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /*
            =====================================================
            عرض نتيجة الطالب
            =====================================================

            طريقة العمل:
            1) نقرأ StudentId من Session
            2) نجيب بيانات الطالب
            3) نستخدم StudentNumber لجلب النتيجة
            4) نعرض النتيجة للطالب فقط
        */
        /*
    =====================================================
    عرض نتيجة الطالب
    =====================================================

    selectedClassRoomName:
    الصف الذي يريد الطالب عرض نتيجته.
*/
        [HttpGet]
        public async Task<IActionResult> Index(string? selectedClassRoomName)
        {
            if (!IsStudentLoggedIn(out string token, out int studentId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await GetCurrentStudent(token, studentId);

            if (student == null || string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                ViewBag.Error = "تعذر تحميل بيانات الطالب.";
                return View(new StudentFullResultViewModel());
            }

            var client = CreateClientWithToken(token);

            var url = $"api/Admin/get-student-full-result/{student.StudentNumber}";

            if (!string.IsNullOrWhiteSpace(selectedClassRoomName))
            {
                url += $"?classRoomName={Uri.EscapeDataString(selectedClassRoomName)}";
            }

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();

                return View(new StudentFullResultViewModel
                {
                    StudentName = student.FullName,
                    StudentNumber = student.StudentNumber,
                    ClassRoomName = student.ClassRoomName ?? "",
                    SectionName = student.SectionName ?? "",
                    SelectedClassRoomName = selectedClassRoomName ?? ""
                });
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<StudentFullResultViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new StudentFullResultViewModel();

            if (string.IsNullOrWhiteSpace(result.SelectedClassRoomName))
            {
                result.SelectedClassRoomName = selectedClassRoomName ?? result.ClassRoomName;
            }

            return View(result);
        }
    }
}