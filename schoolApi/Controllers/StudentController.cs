using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using schoolApi.Data;
using schoolApi.DTOs;
using schoolApi.Models;
using System.Security.Claims;

namespace schoolApi.Controllers
{
    /*
        هذا الكنترولر خاص بالطالب فقط

        الطالب يستطيع من خلاله:
        - عرض بياناته الشخصية
        - عرض مواده
        - عرض درجاته
        - عرض حضوره وغيابه
        - عرض ملاحظاته
        - عرض مهامه اليومية

        لا يسمح إلا للمستخدم الذي دوره Student
    */

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public StudentController(SchoolDbContext context)
        {
            _context = context;
        }

        /*
            =====================================================
            71) API لعرض بيانات الطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض بيانات الطالب
            الذي قام بتسجيل الدخول فقط

            يتم استخراج userId من الـ JWT Token
        */
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // استخراج userId من التوكن
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var user = await _context.Users
                .Include(u => u.Student)
                    .ThenInclude(s => s.ClassRoom)
                .Include(u => u.Student)
                    .ThenInclude(s => s.Section)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null || user.Student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            return Ok(new
            {
                userId = user.Id,
                fullName = user.FullName,
                username = user.Username,
                roleId = user.RoleId,
                studentId = user.Student.Id,
                studentNumber = user.Student.StudentNumber,
                gender = user.Student.Gender,
                birthDate = user.Student.BirthDate,
                address = user.Student.Address,
                phoneNumber = user.Student.PhoneNumber,
                parentName = user.Student.ParentName,
                parentPhone = user.Student.ParentPhone,
                classRoomId = user.Student.ClassRoomId,
                classRoomName = user.Student.ClassRoom != null ? user.Student.ClassRoom.Name : null,
                sectionId = user.Student.SectionId,
                sectionName = user.Student.Section != null ? user.Student.Section.Name : null
            });
        }

        /*
            =====================================================
            72) API لعرض مواد الطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض المواد الخاصة بالطالب الحالي
            من خلال شعبته وصفه
        */
        [HttpGet("my-subjects")]
        public async Task<IActionResult> GetMySubjects()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId));

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var subjects = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                .Where(a => a.ClassRoomId == student.ClassRoomId && a.SectionId == student.SectionId)
                .Select(a => new
                {
                    teacherAssignmentId = a.Id,
                    subjectId = a.SubjectId,
                    subjectName = a.Subject != null ? a.Subject.Name : null,
                    teacherId = a.TeacherId,
                    teacherName = a.Teacher != null && a.Teacher.User != null ? a.Teacher.User.FullName : null,
                    teacherNumber = a.Teacher != null ? a.Teacher.TeacherNumber : null
                })
                .OrderBy(a => a.subjectName)
                .ToListAsync();

            return Ok(subjects);
        }

        /*
            =====================================================
            73) API لعرض درجات الطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع درجات الطالب
            الذي قام بتسجيل الدخول فقط
        */
        [HttpGet("my-grades")]
        public async Task<IActionResult> GetMyGrades()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId));

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Where(g => g.StudentId == student.Id)
                .Select(g => new
                {
                    g.Id,
                    subjectName = g.TeacherAssignment != null && g.TeacherAssignment.Subject != null
                        ? g.TeacherAssignment.Subject.Name
                        : null,
                    teacherName = g.TeacherAssignment != null &&
                                  g.TeacherAssignment.Teacher != null &&
                                  g.TeacherAssignment.Teacher.User != null
                        ? g.TeacherAssignment.Teacher.User.FullName
                        : null,
                    g.Part,
                    g.GradeType,
                    g.Score,
                    g.MaxScore,
                    g.Notes
                })
                .OrderBy(g => g.subjectName)
                .ThenBy(g => g.Part)
                .ThenBy(g => g.GradeType)
                .ToListAsync();

            return Ok(new
            {
                studentId = student.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                grades = grades
            });
        }

        /*
            =====================================================
            74) API لعرض حضور وغياب الطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع سجلات الحضور والغياب
            الخاصة بالطالب الذي قام بتسجيل الدخول فقط
        */
        [HttpGet("my-attendances")]
        public async Task<IActionResult> GetMyAttendances()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId));

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var attendances = await _context.Attendances
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Teacher)
                        .ThenInclude(t => t.User)
                .Where(a => a.StudentId == student.Id)
                .Select(a => new
                {
                    a.Id,
                    subjectName = a.TeacherAssignment != null && a.TeacherAssignment.Subject != null
                        ? a.TeacherAssignment.Subject.Name
                        : null,
                    teacherName = a.TeacherAssignment != null &&
                                  a.TeacherAssignment.Teacher != null &&
                                  a.TeacherAssignment.Teacher.User != null
                        ? a.TeacherAssignment.Teacher.User.FullName
                        : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(new
            {
                studentId = student.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                attendances = attendances
            });
        }

        /*
            =====================================================
            75) API لعرض ملاحظات الطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع الملاحظات
            الخاصة بالطالب الذي قام بتسجيل الدخول فقط
        */
        [HttpGet("my-notes")]
        public async Task<IActionResult> GetMyNotes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId));

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var notes = await _context.StudentNotes
                .Where(n => n.StudentId == student.Id)
                .Select(n => new
                {
                    n.Id,
                    n.NoteText,
                    n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                studentId = student.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                notes = notes
            });
        }

        /*
            =====================================================
            76) API لعرض المهام اليومية للطالب الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع المهام اليومية
            الخاصة بالطالب الذي قام بتسجيل الدخول فقط
        */
        [HttpGet("my-daily-tasks")]
        public async Task<IActionResult> GetMyDailyTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId));

            if (student == null)
            {
                return NotFound("الطالب غير موجود.");
            }

            var tasks = await _context.DailyTasks
                .Where(t => t.StudentId == student.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.TaskDate,
                    t.IsCompleted
                })
                .OrderByDescending(t => t.TaskDate)
                .ToListAsync();

            return Ok(new
            {
                studentId = student.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                dailyTasks = tasks
            });
        }

        /*
     =====================================================
     API تعليم قسم بأنه تمت مشاهدته للطالب الحالي
     =====================================================

     هذا لا يحذف البيانات.
     فقط يحدث LastSeenId حتى يختفي عداد "جديد".
 */
        [HttpPost("mark-my-area-as-viewed/{areaKey}")]
        public async Task<IActionResult> MarkMyAreaAsViewed(string areaKey)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdText) || !int.TryParse(userIdText, out int userId))
            {
                return Unauthorized("تعذر معرفة المستخدم من التوكن.");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود لهذا المستخدم.");
            }

            int maxId = 0;

            if (areaKey == "Lessons")
            {
                maxId = await _context.Lessons
                    .Include(l => l.TeacherAssignment)
                    .Where(l =>
                        l.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        l.TeacherAssignment.SectionId == student.SectionId)
                    .Select(l => l.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Homeworks")
            {
                maxId = await _context.Homeworks
                    .Include(h => h.TeacherAssignment)
                    .Where(h =>
                        h.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        h.TeacherAssignment.SectionId == student.SectionId)
                    .Select(h => h.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Attendance")
            {
                maxId = await _context.Attendances
                    .Where(a => a.StudentId == student.Id)
                    .Select(a => a.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Schedule")
            {
                maxId = await _context.SchoolSchedules
                    .Include(s => s.TeacherAssignment)
                    .Where(s =>
                        s.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                        s.TeacherAssignment.SectionId == student.SectionId)
                    .Select(s => s.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else if (areaKey == "Notes")
            {
                maxId = await _context.StudentNotes
                    .Where(n => n.StudentId == student.Id)
                    .Select(n => n.Id)
                    .DefaultIfEmpty()
                    .MaxAsync();
            }
            else
            {
                return BadRequest("نوع القسم غير صحيح.");
            }

            var status = await _context.StudentPortalReadStatuses
                .FirstOrDefaultAsync(x =>
                    x.StudentId == student.Id &&
                    x.AreaKey == areaKey);

            if (status == null)
            {
                status = new StudentPortalReadStatus
                {
                    StudentId = student.Id,
                    AreaKey = areaKey,
                    LastSeenId = maxId,
                    LastViewedAt = DateTime.Now
                };

                _context.StudentPortalReadStatuses.Add(status);
            }
            else
            {
                status.LastSeenId = maxId;
                status.LastViewedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تحديث حالة المشاهدة بنجاح.",
                studentId = student.Id,
                areaKey,
                lastSeenId = maxId
            });
        }



        /*
    =====================================================
    API عدادات الجديد للطالب الحالي
    =====================================================

    يعتمد على التوكن.
    لا نستقبل StudentId من الواجهة.
*/
        [HttpGet("my-new-counts")]
        public async Task<IActionResult> GetMyNewCounts()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdText) || !int.TryParse(userIdText, out int userId))
            {
                return Unauthorized("تعذر معرفة المستخدم من التوكن.");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود لهذا المستخدم.");
            }

            int GetLastSeen(string areaKey)
            {
                return _context.StudentPortalReadStatuses
                    .Where(x => x.StudentId == student.Id && x.AreaKey == areaKey)
                    .Select(x => x.LastSeenId)
                    .FirstOrDefault();
            }

            var lessonsLastSeen = GetLastSeen("Lessons");
            var homeworksLastSeen = GetLastSeen("Homeworks");
            var attendanceLastSeen = GetLastSeen("Attendance");
            var scheduleLastSeen = GetLastSeen("Schedule");
            var notesLastSeen = GetLastSeen("Notes");

            var lessonsCount = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                .Where(l =>
                    l.Id > lessonsLastSeen &&
                    l.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    l.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            var homeworksCount = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                .Where(h =>
                    h.Id > homeworksLastSeen &&
                    h.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    h.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            var attendanceCount = await _context.Attendances
                .Where(a =>
                    a.Id > attendanceLastSeen &&
                    a.StudentId == student.Id)
                .CountAsync();

            var scheduleCount = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                .Where(s =>
                    s.Id > scheduleLastSeen &&
                    s.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                    s.TeacherAssignment.SectionId == student.SectionId)
                .CountAsync();

            var notesCount = await _context.StudentNotes
                .Where(n =>
                    n.Id > notesLastSeen &&
                    n.StudentId == student.Id)
                .CountAsync();

            return Ok(new
            {
                LessonsCount = lessonsCount,
                HomeworksCount = homeworksCount,
                AttendanceCount = attendanceCount,
                ScheduleCount = scheduleCount,
                NotesCount = notesCount
            });
        }




        /*
    =====================================================
    دالة مساعدة لجلب الطالب الحالي من التوكن
    =====================================================

    التوكن يحتوي على UserId.
    نحن نستخدم UserId للبحث عن الطالب المرتبط بهذا المستخدم.
*/
        private async Task<Student?> GetCurrentStudentFromToken()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdText) || !int.TryParse(userIdText, out int userId))
            {
                return null;
            }

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return student;
        }




        /*
    =====================================================
    API جلب الصفوف التي يوجد للطالب درجات فيها
    =====================================================

    يستخدم في تطبيق Flutter:
    الطالب يختار الصف أولًا.
    الشعبة تظهر تلقائيًا مع الصف.

    الرابط:
    GET api/Student/my-grade-classrooms
*/
        [HttpGet("my-grade-classrooms")]
        public async Task<IActionResult> GetMyGradeClassRooms()
        {
            var student = await GetCurrentStudentFromToken();

            if (student == null)
            {
                return Unauthorized("تعذر معرفة الطالب من التوكن.");
            }

            /*
                نجلب الصفوف والشعب من جدول الدرجات نفسه.
                هذا أفضل من الاعتماد على الصف الحالي فقط،
                لأنه يسمح لاحقًا بعرض درجات صفوف سابقة إذا كانت محفوظة.
            */
            var classRooms = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignment != null)
                .Select(g => new StudentGradeClassRoomDto
                {
                    ClassRoomId = g.TeacherAssignment!.ClassRoomId,
                    ClassRoomName = g.TeacherAssignment.ClassRoom != null
                        ? g.TeacherAssignment.ClassRoom.Name
                        : "",

                    SectionId = g.TeacherAssignment.SectionId,
                    SectionName = g.TeacherAssignment.Section != null
                        ? g.TeacherAssignment.Section.Name
                        : ""
                })
                .Distinct()
                .OrderBy(x => x.ClassRoomName)
                .ToListAsync();

            /*
                إذا لا توجد درجات بعد، نرجع صف الطالب الحالي حتى لا تكون القائمة فارغة.
            */
            if (!classRooms.Any())
            {
                classRooms.Add(new StudentGradeClassRoomDto
                {
                    ClassRoomId = student.ClassRoomId,
                    ClassRoomName = student.ClassRoom != null ? student.ClassRoom.Name : "",
                    SectionId = student.SectionId,
                    SectionName = student.Section != null ? student.Section.Name : ""
                });
            }

            return Ok(classRooms);
        }



        /*
    =====================================================
    دالة مطابقة نوع المحصلة
    =====================================================

    لأن جدول Grade عندك يحتوي على:
    - Part
    - GradeType
    - TermName

    قد تكون القيم محفوظة بالعربي أو بالإنجليزي.
    لذلك نجعل المطابقة مرنة.

    assessmentType:
    First  = المحصلة الأولى
    Second = المحصلة الثانية
*/
        private bool IsMatchingAssessment(Grade grade, string assessmentType)
        {
            var part = grade.Part?.Trim().ToLower() ?? "";
            var gradeType = grade.GradeType?.Trim().ToLower() ?? "";
            var termName = grade.TermName?.Trim().ToLower() ?? "";

            var combined = $"{part} {gradeType} {termName}";

            if (assessmentType == "First")
            {
                return combined.Contains("first")
                       || combined.Contains("1")
                       || combined.Contains("الأولى")
                       || combined.Contains("الاولى")
                       || combined.Contains("المحصلة الأولى")
                       || combined.Contains("المحصله الاولى")
                       || combined.Contains("محصلة أولى")
                       || combined.Contains("محصله اولى");
            }

            if (assessmentType == "Second")
            {
                return combined.Contains("second")
                       || combined.Contains("2")
                       || combined.Contains("الثانية")
                       || combined.Contains("الثانيه")
                       || combined.Contains("المحصلة الثانية")
                       || combined.Contains("المحصله الثانيه")
                       || combined.Contains("محصلة ثانية")
                       || combined.Contains("محصله ثانيه");
            }

            return false;
        }





        /*
    =====================================================
    API جلب درجات الطالب حسب الصف والمحصلة
    =====================================================

    الطالب يرسل:
    - classRoomId
    - assessmentType = First أو Second

    ويرجع:
    - اسم الصف
    - اسم الشعبة
    - اسم المحصلة
    - جميع المواد مع المدرس والدرجة من 20

    الرابط:
    GET api/Student/my-assessment-grades?classRoomId=1&assessmentType=First
*/
        [HttpGet("my-assessment-grades")]
        public async Task<IActionResult> GetMyAssessmentGrades(int classRoomId, string assessmentType)
        {
            var student = await GetCurrentStudentFromToken();

            if (student == null)
            {
                return Unauthorized("تعذر معرفة الطالب من التوكن.");
            }

            if (classRoomId <= 0)
            {
                return BadRequest("يجب اختيار الصف.");
            }

            if (string.IsNullOrWhiteSpace(assessmentType))
            {
                return BadRequest("يجب اختيار نوع المحصلة.");
            }

            assessmentType = assessmentType.Trim();

            if (assessmentType != "First" && assessmentType != "Second")
            {
                return BadRequest("نوع المحصلة غير صحيح. استخدم First أو Second.");
            }

            /*
                نجلب درجات الطالب في الصف المحدد مع:
                - التكليف
                - المادة
                - المدرس
                - الصف
                - الشعبة

                ثم نفلتر المحصلة في الذاكرة لأن Part / GradeType / TermName
                قد تحتوي قيم عربية مختلفة.
            */
            var gradesQuery = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignment != null &&
                    g.TeacherAssignment.ClassRoomId == classRoomId)
                .ToListAsync();

            var grades = gradesQuery
                .Where(g => IsMatchingAssessment(g, assessmentType))
                .ToList();

            var firstGrade = grades.FirstOrDefault();

            /*
                إذا لا توجد درجات للمحصلة المختارة،
                نحتاج مع ذلك إرجاع اسم الصف والشعبة إن أمكن.
            */
            if (firstGrade == null)
            {
                var classInfo = await _context.TeacherAssignments
                    .Include(a => a.ClassRoom)
                    .Include(a => a.Section)
                    .Where(a => a.ClassRoomId == classRoomId)
                    .Select(a => new
                    {
                        a.ClassRoomId,
                        ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : "",
                        a.SectionId,
                        SectionName = a.Section != null ? a.Section.Name : ""
                    })
                    .FirstOrDefaultAsync();

                return Ok(new StudentAssessmentGradesDto
                {
                    ClassRoomId = classRoomId,
                    ClassRoomName = classInfo != null ? classInfo.ClassRoomName : "",
                    SectionId = classInfo != null ? classInfo.SectionId : 0,
                    SectionName = classInfo != null ? classInfo.SectionName : "",
                    AssessmentType = assessmentType,
                    AssessmentTitle = assessmentType == "First" ? "المحصلة الأولى" : "المحصلة الثانية",
                    Items = new List<StudentAssessmentGradeItemDto>()
                });
            }

            /*
                إذا تكرر تسجيل درجة لنفس المادة في نفس المحصلة،
                نأخذ آخر سجل حسب Id.
            */
            var items = grades
                .Where(g => g.TeacherAssignment != null)
                .GroupBy(g => g.TeacherAssignment!.SubjectId)
                .Select(group =>
                {
                    var grade = group
                        .OrderByDescending(g => g.Id)
                        .First();

                    return new StudentAssessmentGradeItemDto
                    {
                        SubjectName = grade.TeacherAssignment?.Subject != null
                            ? grade.TeacherAssignment.Subject.Name
                            : "",

                        TeacherName = grade.TeacherAssignment?.Teacher?.User != null
                            ? grade.TeacherAssignment.Teacher.User.FullName
                            : "",

                        Score = grade.Score,

                        MaxScore = grade.MaxScore > 0 ? grade.MaxScore : 20,

                        Notes = grade.Notes
                    };
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            var result = new StudentAssessmentGradesDto
            {
                ClassRoomId = firstGrade.TeacherAssignment!.ClassRoomId,
                ClassRoomName = firstGrade.TeacherAssignment.ClassRoom != null
                    ? firstGrade.TeacherAssignment.ClassRoom.Name
                    : "",

                SectionId = firstGrade.TeacherAssignment.SectionId,
                SectionName = firstGrade.TeacherAssignment.Section != null
                    ? firstGrade.TeacherAssignment.Section.Name
                    : "",

                AssessmentType = assessmentType,
                AssessmentTitle = assessmentType == "First" ? "المحصلة الأولى" : "المحصلة الثانية",

                Items = items
            };

            return Ok(result);
        }



        /*
    =====================================================
    تحديد نوع الدرجة
    =====================================================

    يعتمد على الحقول الموجودة في جدول Grade:
    - Part
    - GradeType
    - TermName

    ويرجع:
    - FirstAssessment
    - MidYearResult
    - SecondAssessment
    - FinalYearResult
*/
        private string? GetGradeComponentKey(Grade grade)
        {
            var part = grade.Part?.Trim().ToLower() ?? "";
            var gradeType = grade.GradeType?.Trim().ToLower() ?? "";
            var termName = grade.TermName?.Trim().ToLower() ?? "";

            var text = $"{part} {gradeType} {termName}";

            var isAssessment =
                text.Contains("محصلة") ||
                text.Contains("محصله");

            if (
                text.Contains("المحصلة الأولى") ||
                text.Contains("المحصله الاولى") ||
                text.Contains("محصلة أولى") ||
                text.Contains("محصله اولى") ||
                text.Contains("first assessment") ||
                text.Contains("first")
            )
            {
                return "FirstAssessment";
            }

            if (
                text.Contains("المحصلة الثانية") ||
                text.Contains("المحصله الثانيه") ||
                text.Contains("محصلة ثانية") ||
                text.Contains("محصله ثانيه") ||
                text.Contains("second assessment") ||
                text.Contains("second")
            )
            {
                return "SecondAssessment";
            }

            /*
                نتيجة نصف العام:
                لا نريد أن تختلط مع المحصلة الأولى إذا كان TermName يحتوي نصف العام.
                لذلك نستبعد النصوص التي تحتوي محصلة.
            */
            if (
                !isAssessment &&
                (
                    text.Contains("نتيجة نصف العام") ||
                    text.Contains("نصف العام") ||
                    text.Contains("اختبار نصف العام") ||
                    text.Contains("midyear") ||
                    text.Contains("mid year")
                )
            )
            {
                return "MidYearResult";
            }

            /*
                نتيجة نهاية العام:
                لا نريد أن تختلط مع المحصلة الثانية.
            */
            if (
                !isAssessment &&
                (
                    text.Contains("نتيجة نهاية العام") ||
                    text.Contains("نهاية العام") ||
                    text.Contains("اختبار نهاية العام") ||
                    text.Contains("finalyear") ||
                    text.Contains("final year")
                )
            )
            {
                return "FinalYearResult";
            }

            return null;
        }



        /*
    =====================================================
    تحويل المفتاح إلى اسم عربي للعرض
    =====================================================
*/
        private string GetComponentTitle(string key)
        {
            return key switch
            {
                "FirstAssessment" => "المحصلة الأولى",
                "MidYearResult" => "نتيجة نصف العام",
                "SecondAssessment" => "المحصلة الثانية",
                "FinalYearResult" => "نتيجة نهاية العام",
                _ => key
            };
        }




        /*
    =====================================================
    تحديد المكونات المطلوبة حسب نوع النتيجة
    =====================================================

    MidYear:
    - المحصلة الأولى
    - نتيجة نصف العام

    FinalYear:
    - المحصلة الأولى
    - نتيجة نصف العام
    - المحصلة الثانية
    - نتيجة نهاية العام
*/
        private List<string> GetRequiredResultComponents(string resultType)
        {
            if (resultType == "MidYear")
            {
                return new List<string>
        {
            "FirstAssessment",
            "MidYearResult"
        };
            }

            return new List<string>
    {
        "FirstAssessment",
        "MidYearResult",
        "SecondAssessment",
        "FinalYearResult"
    };
        }


        /*
    =====================================================
    API نتيجة الطالب حسب الصف ونوع النتيجة
    =====================================================

    يستخدم في تطبيق Flutter في صفحة "نتيجتي".

    resultType:
    - MidYear   = نتيجة نصف العام
    - FinalYear = نتيجة نهاية العام

    الرابط:
    GET api/Student/my-class-result?classRoomId=1&resultType=MidYear
*/
        [HttpGet("my-class-result")]
        public async Task<IActionResult> GetMyClassResult(int classRoomId, string resultType)
        {
            var student = await GetCurrentStudentFromToken();

            if (student == null)
            {
                return Unauthorized("تعذر معرفة الطالب من التوكن.");
            }

            if (classRoomId <= 0)
            {
                return BadRequest("يجب اختيار الصف.");
            }

            if (string.IsNullOrWhiteSpace(resultType))
            {
                return BadRequest("يجب اختيار نوع النتيجة.");
            }

            resultType = resultType.Trim();

            if (resultType != "MidYear" && resultType != "FinalYear")
            {
                return BadRequest("نوع النتيجة غير صحيح. استخدم MidYear أو FinalYear.");
            }

            var requiredComponents = GetRequiredResultComponents(resultType);

            /*
                نجلب كل درجات الطالب في الصف المحدد.
            */
            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Teacher)
                        .ThenInclude(t => t.User)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignment != null &&
                    g.TeacherAssignment.ClassRoomId == classRoomId)
                .ToListAsync();

            if (!grades.Any())
            {
                return Ok(new StudentClassResultDto
                {
                    ClassRoomId = classRoomId,
                    ClassRoomName = "",
                    SectionId = 0,
                    SectionName = "",
                    ResultType = resultType,
                    ResultTitle = resultType == "MidYear" ? "نتيجة نصف العام" : "نتيجة نهاية العام",
                    TotalScore = 0,
                    TotalMaxScore = 0,
                    Percentage = 0,
                    OverallStatus = "لا توجد درجات",
                    Subjects = new List<StudentResultSubjectDto>()
                });
            }

            var firstGrade = grades.First();

            /*
                نجمع الدرجات حسب المادة.
            */
            var subjects = grades
                .Where(g => g.TeacherAssignment != null)
                .GroupBy(g => g.TeacherAssignment!.SubjectId)
                .Select(group =>
                {
                    var subjectGrades = group.ToList();

                    var anyGrade = subjectGrades
                        .OrderByDescending(g => g.Id)
                        .First();

                    var components = new List<StudentResultComponentDto>();

                    foreach (var componentKey in requiredComponents)
                    {
                        /*
                            نأخذ آخر درجة مسجلة لهذا الجزء.
                        */
                        var grade = subjectGrades
                            .Where(g => GetGradeComponentKey(g) == componentKey)
                            .OrderByDescending(g => g.Id)
                            .FirstOrDefault();

                        components.Add(new StudentResultComponentDto
                        {
                            Key = componentKey,
                            Title = GetComponentTitle(componentKey),
                            Score = grade != null ? grade.Score : 0,
                            MaxScore = grade != null && grade.MaxScore > 0 ? grade.MaxScore : 20
                        });
                    }

                    var totalScore = components.Sum(x => x.Score);
                    var totalMaxScore = components.Sum(x => x.MaxScore);

                    var percentage = totalMaxScore > 0
                        ? Math.Round((totalScore / totalMaxScore) * 100, 2)
                        : 0;

                    /*
                        إذا أي جزء غير موجود، تعتبر المادة غير مكتملة.
                    */
                    var isComplete = requiredComponents.All(componentKey =>
                        subjectGrades.Any(g => GetGradeComponentKey(g) == componentKey));

                    string status;

                    if (!isComplete)
                    {
                        status = "غير مكتمل";
                    }
                    else
                    {
                        status = percentage >= 50 ? "ناجح" : "راسب";
                    }

                    return new StudentResultSubjectDto
                    {
                        SubjectName = anyGrade.TeacherAssignment?.Subject != null
                            ? anyGrade.TeacherAssignment.Subject.Name
                            : "",

                        TeacherName = anyGrade.TeacherAssignment?.Teacher?.User != null
                            ? anyGrade.TeacherAssignment.Teacher.User.FullName
                            : "",

                        Components = components,
                        TotalScore = totalScore,
                        TotalMaxScore = totalMaxScore,
                        Percentage = percentage,
                        Status = status
                    };
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            var totalScoreAll = subjects.Sum(x => x.TotalScore);
            var totalMaxAll = subjects.Sum(x => x.TotalMaxScore);

            var totalPercentage = totalMaxAll > 0
                ? Math.Round((totalScoreAll / totalMaxAll) * 100, 2)
                : 0;

            string overallStatus;

            if (subjects.Any(x => x.Status == "غير مكتمل"))
            {
                overallStatus = "غير مكتمل";
            }
            else
            {
                overallStatus = totalPercentage >= 50 ? "ناجح" : "راسب";
            }

            var result = new StudentClassResultDto
            {
                ClassRoomId = firstGrade.TeacherAssignment!.ClassRoomId,
                ClassRoomName = firstGrade.TeacherAssignment.ClassRoom != null
                    ? firstGrade.TeacherAssignment.ClassRoom.Name
                    : "",

                SectionId = firstGrade.TeacherAssignment.SectionId,
                SectionName = firstGrade.TeacherAssignment.Section != null
                    ? firstGrade.TeacherAssignment.Section.Name
                    : "",

                ResultType = resultType,
                ResultTitle = resultType == "MidYear" ? "نتيجة نصف العام" : "نتيجة نهاية العام",

                TotalScore = totalScoreAll,
                TotalMaxScore = totalMaxAll,
                Percentage = totalPercentage,
                OverallStatus = overallStatus,

                Subjects = subjects
            };

            return Ok(result);
        }




    }
}