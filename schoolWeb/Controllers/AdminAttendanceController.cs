using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using schoolWeb.ViewModels.Attendance;
using schoolWeb.ViewModels.TeacherAssignment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        AdminAttendanceController
        =====================================================

        هذا الكنترولر مسؤول عن إدارة الحضور والغياب من واجهة المدير.

        طريقة العمل:
        - المدير يختار الطالب
        - تظهر بيانات الطالب: الصف والشعبة
        - تظهر المواد الخاصة بصف وشعبة الطالب
        - المدير يختار المادة
        - النظام يبحث عن التكليف التدريسي المناسب تلقائيًا
        - ثم يرسل البيانات إلى API
    */
    public class AdminAttendanceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminAttendanceController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            =====================================================
            دالة إنشاء HttpClient مع التوكن
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
            دالة التحقق من المدير
            =====================================================
        */
        private bool IsAdminLoggedIn(out string token)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            return !string.IsNullOrEmpty(token) && roleName == "Admin";
        }

        /*
            =====================================================
            دالة جلب الطلاب للـ Dropdown
            =====================================================
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
                    Text = $"{s.FullName} - {s.StudentNumber}"
                })
                .ToList();
        }

        /*
            =====================================================
            دالة جلب كل الطلاب
            =====================================================
        */
        private async Task<List<StudentViewModel>> GetStudents(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-students");

            if (!response.IsSuccessStatusCode)
                return new List<StudentViewModel>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<StudentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<StudentViewModel>();
        }

        /*
            =====================================================
            دالة جلب كل التكليفات التدريسية
            =====================================================
        */
        private async Task<List<TeacherAssignmentViewModel>> GetTeacherAssignments(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-teacher-assignments");

            if (!response.IsSuccessStatusCode)
                return new List<TeacherAssignmentViewModel>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<TeacherAssignmentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<TeacherAssignmentViewModel>();
        }

        /*
            =====================================================
            دالة جلب مواد طالب حسب صفه وشعبته
            =====================================================

            هذه الدالة ترجع مواد الطالب فقط.
        */
        private async Task<List<SelectListItem>> GetSubjectsForStudentDropdown(string token, string studentNumber)
        {
            var students = await GetStudents(token);

            var student = students.FirstOrDefault(s =>
                s.StudentNumber.Trim().ToLower() == studentNumber.Trim().ToLower());

            if (student == null)
                return new List<SelectListItem>();

            var assignments = await GetTeacherAssignments(token);

            return assignments
                .Where(a =>
                    a.ClassRoomName.Trim().ToLower() == student.ClassRoomName.Trim().ToLower() &&
                    a.SectionName.Trim().ToLower() == student.SectionName.Trim().ToLower())
                .OrderBy(a => a.SubjectName)
                .Select(a => a.SubjectName)
                .Distinct()
                .Select(subjectName => new SelectListItem
                {
                    Value = subjectName,
                    Text = subjectName
                })
                .ToList();
        }

        /*
            =====================================================
            دالة إيجاد التكليف المناسب
            =====================================================

            تعتمد على:
            - الطالب
            - المادة

            ثم تبحث عن تكليف بنفس:
            - صف الطالب
            - شعبة الطالب
            - المادة المختارة
        */
        private async Task<TeacherAssignmentViewModel?> FindAssignmentForStudentSubject(
            string token,
            string studentNumber,
            string subjectName)
        {
            var students = await GetStudents(token);

            var student = students.FirstOrDefault(s =>
                s.StudentNumber.Trim().ToLower() == studentNumber.Trim().ToLower());

            if (student == null)
                return null;

            var assignments = await GetTeacherAssignments(token);

            return assignments.FirstOrDefault(a =>
                a.SubjectName.Trim().ToLower() == subjectName.Trim().ToLower() &&
                a.ClassRoomName.Trim().ToLower() == student.ClassRoomName.Trim().ToLower() &&
                a.SectionName.Trim().ToLower() == student.SectionName.Trim().ToLower());
        }

        /*
            =====================================================
            Ajax: جلب بيانات الطالب
            =====================================================

            يرجع:
            - الصف
            - الشعبة
        */
        [HttpGet]
        public async Task<IActionResult> GetStudentInfo(string studentNumber)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(studentNumber))
                return Json(null);

            var students = await GetStudents(token);

            var student = students.FirstOrDefault(s =>
                s.StudentNumber.Trim().ToLower() == studentNumber.Trim().ToLower());

            if (student == null)
                return Json(null);

            return Json(new
            {
                classRoom = student.ClassRoomName,
                section = student.SectionName
            });
        }

        /*
            =====================================================
            Ajax: جلب مواد الطالب
            =====================================================

            يرجع المواد التي يدرسها الطالب حسب صفه وشعبته.
        */
        [HttpGet]
        public async Task<IActionResult> GetSubjectsByStudent(string studentNumber)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(studentNumber))
                return Json(new List<object>());

            var students = await GetStudents(token);

            var student = students.FirstOrDefault(s =>
                s.StudentNumber.Trim().ToLower() == studentNumber.Trim().ToLower());

            if (student == null)
                return Json(new List<object>());

            var assignments = await GetTeacherAssignments(token);

            var subjects = assignments
                .Where(a =>
                    a.ClassRoomName.Trim().ToLower() == student.ClassRoomName.Trim().ToLower() &&
                    a.SectionName.Trim().ToLower() == student.SectionName.Trim().ToLower())
                .OrderBy(a => a.SubjectName)
                .Select(a => a.SubjectName)
                .Distinct()
                .Select(subjectName => new
                {
                    subject = subjectName
                })
                .ToList();

            return Json(subjects);
        }

        /*
            =====================================================
            1) عرض سجلات الحضور والغياب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-attendances");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل سجلات الحضور والغياب.";
                return View(new List<AttendanceViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var attendances = JsonSerializer.Deserialize<List<AttendanceViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<AttendanceViewModel>();

            return View(attendances);
        }

        /*
            =====================================================
            2) فتح صفحة إضافة حضور أو غياب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var model = new CreateAttendanceViewModel
            {
                AttendanceDate = DateTime.Today,
                Students = await GetStudentsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            3) تنفيذ إضافة حضور أو غياب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateAttendanceViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            var assignment = await FindAssignmentForStudentSubject(
                token,
                model.StudentNumber,
                model.SubjectName);

            if (assignment == null)
            {
                ModelState.AddModelError("", "لم يتم العثور على تكليف تدريسي مناسب لهذا الطالب وهذه المادة.");

                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            var client = CreateClientWithToken(token);

            /*
                API الحضور يحتاج:
                - StudentNumber
                - TeacherNumber
                - SubjectName
                - ClassRoomName
                - SectionName
                - AttendanceDate
                - Status
                - Notes
            */
            var requestData = new
            {
                studentNumber = model.StudentNumber,
                teacherNumber = assignment.TeacherNumber,
                subjectName = assignment.SubjectName,
                classRoomName = assignment.ClassRoomName,
                sectionName = assignment.SectionName,
                attendanceDate = model.AttendanceDate,
                status = model.Status,
                notes = model.Notes
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-attendance", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.Students = await GetStudentsForDropdown(token);
                return View(model);
            }

            TempData["Success"] = "تم إضافة سجل الحضور/الغياب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            4) فتح صفحة تعديل حضور أو غياب
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-attendance/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل سجل الحضور/الغياب.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var attendance = JsonSerializer.Deserialize<AttendanceViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (attendance == null)
            {
                TempData["Error"] = "سجل الحضور/الغياب غير موجود.";
                return RedirectToAction("Index");
            }

            var model = new UpdateAttendanceViewModel
            {
                Id = attendance.Id,
                StudentNumber = attendance.StudentNumber ?? "",
                SubjectName = attendance.SubjectName ?? "",
                AttendanceDate = attendance.AttendanceDate,
                Status = attendance.Status,
                Notes = attendance.Notes,
                Students = await GetStudentsForDropdown(token),
                Subjects = await GetSubjectsForStudentDropdown(token, attendance.StudentNumber ?? "")
            };

            return View(model);
        }

        /*
            =====================================================
            5) تنفيذ تعديل حضور أو غياب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateAttendanceViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Students = await GetStudentsForDropdown(token);
                model.Subjects = await GetSubjectsForStudentDropdown(token, model.StudentNumber);
                return View(model);
            }

            var assignment = await FindAssignmentForStudentSubject(
                token,
                model.StudentNumber,
                model.SubjectName);

            if (assignment == null)
            {
                ModelState.AddModelError("", "لم يتم العثور على تكليف تدريسي مناسب لهذا الطالب وهذه المادة.");

                model.Students = await GetStudentsForDropdown(token);
                model.Subjects = await GetSubjectsForStudentDropdown(token, model.StudentNumber);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = model.StudentNumber,
                teacherNumber = assignment.TeacherNumber,
                subjectName = assignment.SubjectName,
                classRoomName = assignment.ClassRoomName,
                sectionName = assignment.SectionName,
                attendanceDate = model.AttendanceDate,
                status = model.Status,
                notes = model.Notes
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-attendance/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", apiError);

                model.Students = await GetStudentsForDropdown(token);
                model.Subjects = await GetSubjectsForStudentDropdown(token, model.StudentNumber);

                return View(model);
            }

            TempData["Success"] = "تم تعديل سجل الحضور/الغياب بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            6) حذف حضور أو غياب
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-attendance/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = apiError;
            }
            else
            {
                TempData["Success"] = "تم حذف سجل الحضور/الغياب بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}