using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using schoolWeb.ViewModels;
using schoolWeb.ViewModels.AdminSchoolSchedule;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace schoolWeb.Controllers
{
    /*
        =====================================================
        AdminSchoolScheduleController
        =====================================================

        هذا الكنترولر مسؤول عن إدارة الجدول الدراسي من قبل المدير.

        المدير يستطيع:
        - عرض الجدول
        - إضافة حصة
        - تعديل حصة
        - حذف حصة
    */
    public class AdminSchoolScheduleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminSchoolScheduleController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /*
            التحقق من أن المستخدم الحالي مدير
        */
        private bool IsAdminLoggedIn(out string token)
        {
            token = HttpContext.Session.GetString("Token") ?? "";
            var roleName = HttpContext.Session.GetString("RoleName");

            return !string.IsNullOrWhiteSpace(token) && roleName == "Admin";
        }

        /*
            تجهيز الاتصال مع API مع التوكن
        */
        private HttpClient CreateClientWithToken(string token)
        {
            var client = _httpClientFactory.CreateClient("SchoolApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /*
            أيام الأسبوع للجدول الدراسي
        */
        private List<SelectListItem> GetDays(string? selectedDay = null)
        {
            var days = new List<string>
            {
                "السبت",
                "الأحد",
                "الاثنين",
                "الثلاثاء",
                "الأربعاء",
                "الخميس"
            };

            return days.Select(day => new SelectListItem
            {
                Value = day,
                Text = day,
                Selected = day == selectedDay
            }).ToList();
        }

        /*
            أرقام الحصص
        */
        private List<SelectListItem> GetPeriods(int selectedPeriod = 0)
        {
            return Enumerable.Range(1, 8)
                .Select(period => new SelectListItem
                {
                    Value = period.ToString(),
                    Text = $"الحصة {period}",
                    Selected = period == selectedPeriod
                })
                .ToList();
        }

        /*
            جلب الصفوف من API
        */
        private async Task<List<SelectListItem>> GetClassRooms(string token, int selectedId = 0)
        {
            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-classrooms");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var classRooms = JsonSerializer.Deserialize<List<ClassRoomViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ClassRoomViewModel>();

            return classRooms.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == selectedId
            }).ToList();
        }

        /*
     جلب الشعب حسب الصف المختار

     إذا لم يتم اختيار الصف، لا نعرض أي شعبة.
     إذا تم اختيار الصف، نعرض فقط الشعب المرتبطة بهذا الصف.
 */
        private async Task<List<SelectListItem>> GetSections(string token, int classRoomId, int selectedId = 0)
        {
            if (classRoomId <= 0)
            {
                return new List<SelectListItem>();
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-sections");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var sections = JsonSerializer.Deserialize<List<SectionViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<SectionViewModel>();

            /*
                نعرض فقط الشعب التي تتبع الصف المختار
            */
            sections = sections
                .Where(s => s.ClassRoomId == classRoomId)
                .OrderBy(s => s.SectionName)
                .ToList();

            return sections.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.SectionName,
                Selected = s.Id == selectedId
            }).ToList();
        }

        /*
            جلب التكليفات حسب الصف والشعبة

            إذا لم يتم اختيار الصف والشعبة بعد، نرجع قائمة فارغة.
        */
        private async Task<List<SelectListItem>> GetTeacherAssignments(
            string token,
            int classRoomId,
            int sectionId,
            int selectedAssignmentId = 0)
        {
            if (classRoomId <= 0 || sectionId <= 0)
            {
                return new List<SelectListItem>();
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync(
                $"api/Admin/get-assignments-by-class-section?classRoomId={classRoomId}&sectionId={sectionId}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var assignments = JsonSerializer.Deserialize<List<ScheduleAssignmentViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ScheduleAssignmentViewModel>();

            return assignments.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.SubjectName} - {a.TeacherName}",
                Selected = a.Id == selectedAssignmentId
            }).ToList();
        }

        /*
            =====================================================
            عرض الجدول الدراسي
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync("api/Admin/get-school-schedules");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "تعذر تحميل الجدول الدراسي.";
                return View(new List<SchoolScheduleViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var schedules = JsonSerializer.Deserialize<List<SchoolScheduleViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<SchoolScheduleViewModel>();

            return View(schedules);
        }

        /*
            =====================================================
            فتح صفحة إضافة حصة
            =====================================================
        */
        [HttpGet]
        public async Task<IActionResult> Create(int classRoomId = 0, int sectionId = 0)
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new CreateSchoolScheduleViewModel
            {
                ClassRoomId = classRoomId,
                SectionId = sectionId,
                ClassRooms = await GetClassRooms(token, classRoomId),
                Sections = await GetSections(token, classRoomId, sectionId),
                TeacherAssignments = await GetTeacherAssignments(token, classRoomId, sectionId),
                Days = GetDays(),
                Periods = GetPeriods()
            };

            return View(model);
        }

        /*
            =====================================================
            تنفيذ إضافة حصة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Create(CreateSchoolScheduleViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.ClassRooms = await GetClassRooms(token, model.ClassRoomId);
                model.Sections = await GetSections(token, model.ClassRoomId, model.SectionId);
                model.TeacherAssignments = await GetTeacherAssignments(token, model.ClassRoomId, model.SectionId, model.TeacherAssignmentId);
                model.Days = GetDays(model.DayName);
                model.Periods = GetPeriods(model.PeriodNumber);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherAssignmentId = model.TeacherAssignmentId,
                dayName = model.DayName,
                periodNumber = model.PeriodNumber,
                startTime = model.StartTime,
                endTime = model.EndTime,
                roomName = model.RoomName,
                notes = model.Notes
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Admin/create-school-schedule", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", error);

                model.ClassRooms = await GetClassRooms(token, model.ClassRoomId);
                model.Sections = await GetSections(token, model.SectionId);
                model.TeacherAssignments = await GetTeacherAssignments(token, model.ClassRoomId, model.SectionId, model.TeacherAssignmentId);
                model.Days = GetDays(model.DayName);
                model.Periods = GetPeriods(model.PeriodNumber);

                return View(model);
            }

            TempData["Success"] = "تم إضافة الحصة إلى الجدول الدراسي بنجاح.";
            return RedirectToAction("Index");
        }

        /*
     =====================================================
     فتح صفحة تعديل حصة
     =====================================================

     classRoomId و sectionId اختياريين:
     نستخدمهم إذا غير المدير الصف أو الشعبة داخل صفحة التعديل
     حتى نعيد تحميل التكليفات المناسبة.
 */
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int classRoomId = 0, int sectionId = 0)
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var response = await client.GetAsync($"api/Admin/get-school-schedule/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "تعذر تحميل بيانات الحصة.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var schedule = JsonSerializer.Deserialize<UpdateSchoolScheduleViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (schedule == null)
            {
                TempData["Error"] = "الحصة غير موجودة.";
                return RedirectToAction("Index");
            }

            /*
                إذا جاء classRoomId أو sectionId من الصفحة نستخدمه
                وإلا نستخدم القيم الأصلية القادمة من API.
            */
            if (classRoomId > 0)
            {
                schedule.ClassRoomId = classRoomId;
            }

            if (sectionId > 0)
            {
                schedule.SectionId = sectionId;
            }

            schedule.ClassRooms = await GetClassRooms(token, schedule.ClassRoomId);
            schedule.Sections = await GetSections(token, schedule.ClassRoomId, schedule.SectionId);
            schedule.TeacherAssignments = await GetTeacherAssignments(
                token,
                schedule.ClassRoomId,
                schedule.SectionId,
                schedule.TeacherAssignmentId);

            schedule.Days = GetDays(schedule.DayName);
            schedule.Periods = GetPeriods(schedule.PeriodNumber);

            return View(schedule);
        }

        /*
            =====================================================
            تنفيذ تعديل حصة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateSchoolScheduleViewModel model)
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.ClassRooms = await GetClassRooms(token, model.ClassRoomId);
                model.Sections = await GetSections(token, model.ClassRoomId, model.SectionId);
                model.TeacherAssignments = await GetTeacherAssignments(token, model.ClassRoomId, model.SectionId, model.TeacherAssignmentId);
                model.Days = GetDays(model.DayName);
                model.Periods = GetPeriods(model.PeriodNumber);

                return View(model);
            }

            var client = CreateClientWithToken(token);

            var requestData = new
            {
                teacherAssignmentId = model.TeacherAssignmentId,
                dayName = model.DayName,
                periodNumber = model.PeriodNumber,
                startTime = model.StartTime,
                endTime = model.EndTime,
                roomName = model.RoomName,
                notes = model.Notes
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/Admin/update-school-schedule/{model.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", error);

                model.ClassRooms = await GetClassRooms(token, model.ClassRoomId);
                model.Sections = await GetSections(token, model.SectionId);
                model.TeacherAssignments = await GetTeacherAssignments(token, model.ClassRoomId, model.SectionId, model.TeacherAssignmentId);
                model.Days = GetDays(model.DayName);
                model.Periods = GetPeriods(model.PeriodNumber);

                return View(model);
            }

            TempData["Success"] = "تم تعديل الحصة بنجاح.";
            return RedirectToAction("Index");
        }

        /*
            =====================================================
            حذف حصة
            =====================================================
        */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn(out string token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = CreateClientWithToken(token);

            var response = await client.DeleteAsync($"api/Admin/delete-school-schedule/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                TempData["Success"] = "تم حذف الحصة من الجدول الدراسي بنجاح.";
            }

            return RedirectToAction("Index");
        }
    }
}