using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using schoolWeb.ViewModels.Grade;
using schoolWeb.ViewModels.TeacherAssignment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        AdminGradeController
        =====================================================

        هذا الكنترولر مسؤول عن:
        - إدارة درجات نصف العام
        - إدارة درجات نهاية العام
        - اختيار الصف والشعبة والمادة
        - عرض الطلاب حسب الصف والشعبة
        - حفظ درجة طالب واحد في المادة المختارة
    */
    public class AdminGradeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminGradeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
            التحقق من المدير
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
            جلب الصفوف
            =====================================================
        */
        private async Task<List<SelectListItem>> GetClassRoomsForDropdown(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
                return new List<SelectListItem>();

            var json = await response.Content.ReadAsStringAsync();

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ClassRoomViewModel>();

            return classRooms
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                })
                .ToList();
        }

        /*
            =====================================================
            جلب الشعب
            =====================================================
        */
        private async Task<List<SectionViewModel>> GetSections(string token)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
                return new List<SectionViewModel>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<SectionViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<SectionViewModel>();
        }

        /*
            =====================================================
            جلب الطلاب
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
            جلب التكليفات التدريسية
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
            1) الصفحة الرئيسية للدرجات
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-grades");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الدرجات.";
                return View(new List<GradeGroupViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var grades = JsonSerializer.Deserialize<List<GradeListViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<GradeListViewModel>();

            var groupedGrades = grades
                .GroupBy(g => new
                {
                    g.StudentNumber,
                    g.StudentName,
                    g.SubjectName,
                    g.ClassRoomName,
                    g.SectionName,
                    g.TermName
                })
                .Select(group =>
                {
                    var assessment = group.FirstOrDefault(x =>
                        x.GradeType == "المحصلة الأولى" ||
                        x.GradeType == "المحصلة الثانية");

                    var exam = group.FirstOrDefault(x =>
                        x.GradeType == "اختبار" ||
                        x.GradeType == "اختبار نصف العام" ||
                        x.GradeType == "اختبار نهاية العام");

                    return new GradeGroupViewModel
                    {
                        FirstGradeId = assessment?.Id ?? 0,
                        ExamGradeId = exam?.Id ?? 0,

                        StudentName = group.Key.StudentName,
                        StudentNumber = group.Key.StudentNumber,
                        SubjectName = group.Key.SubjectName,
                        ClassRoomName = group.Key.ClassRoomName,
                        SectionName = group.Key.SectionName,
                        TermName = group.Key.TermName,

                        AssessmentScore = assessment?.Score ?? 0,
                        ExamScore = exam?.Score ?? 0
                    };
                })
                .OrderBy(g => g.ClassRoomName)
                .ThenBy(g => g.SectionName)
                .ThenBy(g => g.StudentName)
                .ThenBy(g => g.SubjectName)
                .ThenBy(g => g.TermName)
                .ToList();

            return View(groupedGrades);
        }

        /*
            =====================================================
            2) صفحة إدخال درجات نصف العام
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> CreateMidYear()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var model = new MidYearGradePageViewModel
            {
                ClassRooms = await GetClassRoomsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            3) صفحة إدخال درجات نهاية العام
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> CreateFinalYear()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var model = new FinalYearGradePageViewModel
            {
                ClassRooms = await GetClassRoomsForDropdown(token)
            };

            return View(model);
        }

        /*
            =====================================================
            Ajax: جلب الشعب حسب الصف
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> GetSectionsByClassRoom(string classRoomName)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(classRoomName))
                return Json(new List<object>());

            var sections = await GetSections(token);

            var result = sections
                .Where(s => s.ClassRoomName.Trim().ToLower() == classRoomName.Trim().ToLower())
                .OrderBy(s => s.SectionName)
                .Select(s => new
                {
                    sectionName = s.SectionName
                })
                .ToList();

            return Json(result);
        }

        /*
            =====================================================
            Ajax: جلب المواد حسب الصف والشعبة
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> GetSubjectsByClassRoomAndSection(string classRoomName, string sectionName)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(classRoomName) || string.IsNullOrWhiteSpace(sectionName))
                return Json(new List<object>());

            var assignments = await GetTeacherAssignments(token);

            var result = assignments
                .Where(a =>
                    a.ClassRoomName.Trim().ToLower() == classRoomName.Trim().ToLower() &&
                    a.SectionName.Trim().ToLower() == sectionName.Trim().ToLower())
                .OrderBy(a => a.SubjectName)
                .Select(a => a.SubjectName)
                .Distinct()
                .Select(subjectName => new
                {
                    subjectName
                })
                .ToList();

            return Json(result);
        }

        /*
            =====================================================
            Ajax: جلب الطلاب حسب الصف والشعبة
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> GetStudentsByClassRoomAndSection(string classRoomName, string sectionName)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(classRoomName) || string.IsNullOrWhiteSpace(sectionName))
                return Json(new List<object>());

            var students = await GetStudents(token);

            var result = students
                .Where(s =>
                    s.ClassRoomName.Trim().ToLower() == classRoomName.Trim().ToLower() &&
                    s.SectionName.Trim().ToLower() == sectionName.Trim().ToLower())
                .OrderBy(s => s.FullName)
                .Select(s => new
                {
                    studentNumber = s.StudentNumber,
                    fullName = s.FullName
                })
                .ToList();

            return Json(result);
        }

        /*
            =====================================================
            4) حفظ درجة نصف العام لطالب واحد
            =====================================================

            المحصلة الأولى من 20
            اختبار نصف العام من 30
            المجموع من 50
        */
        [HttpPost]
        public async Task<IActionResult> SaveMidYearStudentGrade([FromBody] MidYearStudentGradeSaveDto dto)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (dto.FirstAssessmentScore < 0 || dto.FirstAssessmentScore > 20)
                return BadRequest("المحصلة الأولى يجب أن تكون من 0 إلى 20.");

            if (dto.MidtermExamScore < 0 || dto.MidtermExamScore > 30)
                return BadRequest("اختبار نصف العام يجب أن يكون من 0 إلى 30.");

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = dto.StudentNumber,
                subjectName = dto.SubjectName,
                classRoomName = dto.ClassRoomName,
                sectionName = dto.SectionName,
                termName = "نصف العام",
                firstAssessmentScore = dto.FirstAssessmentScore,
                midtermExamScore = dto.MidtermExamScore
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-midyear-grade", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest(error);
            }

            return Ok(new
            {
                message = "تم حفظ درجة نصف العام بنجاح.",
                total = dto.FirstAssessmentScore + dto.MidtermExamScore
            });
        }

        /*
            =====================================================
            5) حفظ درجة نهاية العام لطالب واحد
            =====================================================

            المحصلة الثانية من 20
            اختبار نهاية العام من 30
            المجموع من 50
        */
        [HttpPost]
        public async Task<IActionResult> SaveFinalYearStudentGrade([FromBody] FinalYearStudentGradeSaveDto dto)
        {
            if (!IsAdminLoggedIn(out string token))
                return Unauthorized();

            if (dto.SecondAssessmentScore < 0 || dto.SecondAssessmentScore > 20)
                return BadRequest("المحصلة الثانية يجب أن تكون من 0 إلى 20.");

            if (dto.FinalExamScore < 0 || dto.FinalExamScore > 30)
                return BadRequest("اختبار نهاية العام يجب أن يكون من 0 إلى 30.");

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = dto.StudentNumber,
                subjectName = dto.SubjectName,
                classRoomName = dto.ClassRoomName,
                sectionName = dto.SectionName,
                termName = "نهاية العام",
                secondAssessmentScore = dto.SecondAssessmentScore,
                finalExamScore = dto.FinalExamScore
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-finalyear-grade", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return BadRequest(error);
            }

            return Ok(new
            {
                message = "تم حفظ درجة نهاية العام بنجاح.",
                total = dto.SecondAssessmentScore + dto.FinalExamScore
            });
        }

        /*
    =====================================================
    فتح صفحة تعديل درجة مجمعة
    =====================================================
*/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            // نجيب الدرجة التي تم الضغط عليها
            var response = await client.GetAsync($"api/Admin/get-grade/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الدرجة.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var selectedGrade = JsonSerializer.Deserialize<GradeListViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (selectedGrade == null)
            {
                TempData["Error"] = "الدرجة غير موجودة.";
                return RedirectToAction("Index");
            }

            // نجيب كل الدرجات عشان نأخذ المحصلة والاختبار لنفس الطالب والمادة والفترة
            var allResponse = await client.GetAsync("api/Admin/get-grades");

            if (!allResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل باقي درجات الطالب.";
                return RedirectToAction("Index");
            }

            var allJson = await allResponse.Content.ReadAsStringAsync();

            var allGrades = JsonSerializer.Deserialize<List<GradeListViewModel>>(allJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<GradeListViewModel>();

            var relatedGrades = allGrades
                .Where(g =>
                    g.StudentNumber == selectedGrade.StudentNumber &&
                    g.SubjectName == selectedGrade.SubjectName &&
                    g.ClassRoomName == selectedGrade.ClassRoomName &&
                    g.SectionName == selectedGrade.SectionName &&
                    g.TermName == selectedGrade.TermName)
                .ToList();

            var assessmentGrade = relatedGrades.FirstOrDefault(g =>
                g.GradeType == "المحصلة الأولى" ||
                g.GradeType == "المحصلة الثانية");

            var examGrade = relatedGrades.FirstOrDefault(g =>
                g.GradeType == "اختبار" ||
                g.GradeType == "اختبار نصف العام" ||
                g.GradeType == "اختبار نهاية العام");

            var model = new UpdateGradeViewModel
            {
                Id = selectedGrade.Id,
                StudentNumber = selectedGrade.StudentNumber,
                SubjectName = selectedGrade.SubjectName,
                ClassRoomName = selectedGrade.ClassRoomName,
                SectionName = selectedGrade.SectionName,
                TermName = selectedGrade.TermName
            };

            if (selectedGrade.TermName == "نصف العام")
            {
                model.FirstAssessmentScore = assessmentGrade?.Score ?? 0;
                model.MidtermExamScore = examGrade?.Score ?? 0;
            }
            else if (selectedGrade.TermName == "نهاية العام")
            {
                model.SecondAssessmentScore = assessmentGrade?.Score ?? 0;
                model.FinalExamScore = examGrade?.Score ?? 0;
            }

            return View(model);
        }

        /*
            =====================================================
            تنفيذ تعديل درجة مجمعة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateGradeViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                studentNumber = model.StudentNumber,
                subjectName = model.SubjectName,
                classRoomName = model.ClassRoomName,
                sectionName = model.SectionName,
                termName = model.TermName,

                firstAssessmentScore = model.FirstAssessmentScore,
                midtermExamScore = model.MidtermExamScore,
                secondAssessmentScore = model.SecondAssessmentScore,
                finalExamScore = model.FinalExamScore
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-grade/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["Success"] = "تم تعديل الدرجة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            8) حذف درجة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-grade/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "تم حذف الدرجة بنجاح.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int firstGradeId, int examGradeId)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken(token);

            if (firstGradeId > 0)
            {
                await client.DeleteAsync($"api/Admin/delete-grade/{firstGradeId}");
            }

            if (examGradeId > 0)
            {
                await client.DeleteAsync($"api/Admin/delete-grade/{examGradeId}");
            }

            TempData["Success"] = "تم حذف درجات المادة لهذه الفترة بنجاح.";
            return RedirectToAction("Index");
        }


        private async Task<List<SelectListItem>> GetStudentsForResultDropdown(string token)
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





        [HttpGet]
        public async Task<IActionResult> StudentResult()
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            var model = new StudentFullResultViewModel
            {
                Students = await GetStudentsForResultDropdown(token)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StudentResult(StudentFullResultViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
                return RedirectToAction("Login", "Auth");

            model.Students = await GetStudentsForResultDropdown(token);

            if (string.IsNullOrWhiteSpace(model.StudentNumber))
            {
                ViewBag.Error = "يرجى اختيار الطالب.";
                return View(model);
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-student-full-result/{model.StudentNumber}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<StudentFullResultViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                ViewBag.Error = "تعذر تحميل النتيجة.";
                return View(model);
            }

            result.Students = model.Students;

            return View(result);
        }
    }
}