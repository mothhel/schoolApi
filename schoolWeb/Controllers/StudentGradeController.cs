using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels.StudentPortal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        StudentGradeController
        =====================================================

        الطالب يختار:
        1) الصف
        2) المحصلة

        ثم تظهر له الدرجات الخاصة بهذا الاختيار.
    */
    public class StudentGradeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentGradeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

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

        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /*
            جلب بيانات الطالب الحالي
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
            خيارات المحصلة
        */
        private List<SelectListItem> GetGradeTypeOptions(string selectedGradeType)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "المحصلة الأولى",
                    Text = "المحصلة الأولى",
                    Selected = selectedGradeType == "المحصلة الأولى"
                },
                new SelectListItem
                {
                    Value = "المحصلة الثانية",
                    Text = "المحصلة الثانية",
                    Selected = selectedGradeType == "المحصلة الثانية"
                }
            };
        }

        /*
            تجهيز قائمة الصفوف

            حاليًا الطالب له صف واحد.
            لكن جعلناها قائمة حتى تكون جاهزة لاحقًا إذا احتجنا
            عرض سنوات أو صفوف سابقة.
        */
        private List<SelectListItem> GetClassRoomOptions(string classRoomName, string? selectedClassRoomName)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = classRoomName,
                    Text = classRoomName,
                    Selected = selectedClassRoomName == classRoomName
                }
            };
        }

        /*
            =====================================================
            عرض درجات الطالب
            =====================================================

            selectedClassRoomName:
            الصف المختار

            selectedGradeType:
            المحصلة المختارة
        */
        [HttpGet]
        public async Task<IActionResult> Index(
            string? selectedClassRoomName,
            string? selectedGradeType)
        {
            if (!IsStudentLoggedIn(out string token, out int studentId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await GetCurrentStudent(token, studentId);

            if (student == null)
            {
                ViewBag.Error = "تعذر تحميل بيانات الطالب.";
                return View(new List<StudentGradeViewModel>());
            }

            selectedClassRoomName ??= "";
            selectedGradeType ??= "";

            ViewBag.StudentName = student.FullName;
            ViewBag.ClassRoomName = student.ClassRoomName;
            ViewBag.SectionName = student.SectionName;

            ViewBag.SelectedClassRoomName = selectedClassRoomName;
            ViewBag.SelectedGradeType = selectedGradeType;

            ViewBag.ClassRoomOptions = GetClassRoomOptions(student.ClassRoomName ?? "", selectedClassRoomName);
            ViewBag.GradeTypeOptions = GetGradeTypeOptions(selectedGradeType);

            /*
                إذا لم يختر الطالب الصف أو المحصلة
                لا نعرض الدرجات بعد.
            */
            if (string.IsNullOrWhiteSpace(selectedClassRoomName) ||
                string.IsNullOrWhiteSpace(selectedGradeType))
            {
                return View(new List<StudentGradeViewModel>());
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student-grades/{studentId}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل درجات الطالب.";
                return View(new List<StudentGradeViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var grades = JsonSerializer.Deserialize<List<StudentGradeViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<StudentGradeViewModel>();

            /*
                نعرض فقط:
                - الصف المختار
                - المحصلة المختارة
            */
            grades = grades
                .Where(g =>
                    g.ClassRoomName == selectedClassRoomName &&
                    g.GradeType == selectedGradeType)
                .OrderBy(g => g.SubjectName)
                .ToList();

            return View(grades);
        }
    }
}