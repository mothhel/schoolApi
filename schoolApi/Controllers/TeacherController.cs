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
        هذا الكنترولر خاص بالمدرس فقط

        المدرس يستطيع من خلاله:
        - عرض بياناته الشخصية
        - عرض تكليفاته التدريسية
        - عرض دروسه
        - عرض واجباته
        - عرض الطلاب المرتبطين بتكليفاته
        - عرض درجات الطلاب في مواده
        - عرض حضور وغياب الطلاب في مواده

        لا يسمح إلا للمستخدم الذي دوره Teacher
    */

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public TeacherController(SchoolDbContext context)
        {
            _context = context;
        }

        /*
            =====================================================
            77) API لعرض بيانات المدرس الحالي
            =====================================================

            هذا الـ endpoint يعرض بيانات المدرس
            الذي قام بتسجيل الدخول فقط
        */
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var user = await _context.Users
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null || user.Teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            return Ok(new
            {
                userId = user.Id,
                fullName = user.FullName,
                username = user.Username,
                roleId = user.RoleId,
                teacherId = user.Teacher.Id,
                teacherNumber = user.Teacher.TeacherNumber,
                specialty = user.Teacher.Specialty,
                phoneNumber = user.Teacher.PhoneNumber,
                address = user.Teacher.Address,
                hireDate = user.Teacher.HireDate
            });
        }

        /*
            =====================================================
            78) API لعرض تكليفات المدرس الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع المواد
            والصفوف والشعب الخاصة بالمدرس الحالي
        */
        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var teacher = await GetCurrentTeacherFromToken();

            if (teacher == null)
            {
                return Unauthorized("تعذر معرفة الأستاذ من التوكن.");
            }

            var assignments = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Where(a => a.TeacherId == teacher.Id)
                .Select(a => new TeacherAssignmentSelectDto
                {
                    Id = a.Id,

                    SubjectName = a.Subject != null
                        ? a.Subject.Name
                        : "",

                    ClassRoomName = a.ClassRoom != null
                        ? a.ClassRoom.Name
                        : "",

                    SectionName = a.Section != null
                        ? a.Section.Name
                        : "",

                    DisplayName =
                        (a.Subject != null ? a.Subject.Name : "") +
                        " - " +
                        (a.ClassRoom != null ? a.ClassRoom.Name : "") +
                        " - " +
                        (a.Section != null ? a.Section.Name : "")
                })
                .OrderBy(a => a.ClassRoomName)
                .ThenBy(a => a.SectionName)
                .ThenBy(a => a.SubjectName)
                .ToListAsync();

            return Ok(assignments);
        }

        /*
     =====================================================
     API عرض دروس الأستاذ الحالي
     =====================================================

     يعرض فقط الدروس المرتبطة بتكليفات هذا الأستاذ.

     مهم:
     لا نستدعي GetArabicDayName داخل Select مباشرة
     لأن Entity Framework لا يستطيع ترجمة هذه الدالة داخل الاستعلام.

     لذلك:
     1) نجلب الدروس من قاعدة البيانات أولًا.
     2) ثم نحولها إلى TeacherLessonDto داخل الذاكرة.
 */
        [HttpGet("my-lessons")]
        public async Task<IActionResult> GetMyLessons()
        {
            var teacher = await GetCurrentTeacherFromToken();

            if (teacher == null)
            {
                return Unauthorized("تعذر معرفة الأستاذ من التوكن.");
            }

            var lessonsFromDb = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(l => l.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(l =>
                    l.TeacherAssignment != null &&
                    l.TeacherAssignment.TeacherId == teacher.Id)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            var lessons = lessonsFromDb
                .Select(l => new TeacherLessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    LessonDate = l.LessonDate,

                    CreatedAt = l.CreatedAt,
                    CreatedDayName = GetArabicDayName(l.CreatedAt),
                    CreatedTime = l.CreatedAt.ToString("hh:mm tt"),

                    UpdatedAt = l.UpdatedAt,
                    UpdatedDayName = l.UpdatedAt.HasValue ? GetArabicDayName(l.UpdatedAt.Value) : "",
                    UpdatedTime = l.UpdatedAt.HasValue ? l.UpdatedAt.Value.ToString("hh:mm tt") : "",

                    TeacherAssignmentId = l.TeacherAssignmentId,

                    SubjectName = l.TeacherAssignment?.Subject != null
                        ? l.TeacherAssignment.Subject.Name
                        : "",

                    ClassRoomName = l.TeacherAssignment?.ClassRoom != null
                        ? l.TeacherAssignment.ClassRoom.Name
                        : "",

                    SectionName = l.TeacherAssignment?.Section != null
                        ? l.TeacherAssignment.Section.Name
                        : ""
                })
                .ToList();

            return Ok(lessons);
        }

        /*
            =====================================================
            80) API لعرض واجبات المدرس الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع الواجبات
            المرتبطة بتكليفات المدرس الحالي
        */
        [HttpGet("my-homeworks")]
        public async Task<IActionResult> GetMyHomeworks()
        {
            /*
                =====================================================
                API عرض واجبات الأستاذ الحالي
                =====================================================

                هذا الأكشن يرجع واجبات الأستاذ فقط حسب التوكن.

                أضفنا:
                - وقت الإضافة
                - يوم الإضافة
                - ساعة الإضافة
                - وقت آخر تعديل
                - يوم آخر تعديل
                - ساعة آخر تعديل

                ملاحظة مهمة:
                عملنا ToListAsync أولًا ثم Select
                حتى لا يحدث خطأ مع دالة GetArabicDayName داخل استعلام قاعدة البيانات.
            */

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var homeworksFromDb = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(h => h.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(h =>
                    h.TeacherAssignment != null &&
                    h.TeacherAssignment.TeacherId == teacher.Id)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var homeworks = homeworksFromDb.Select(h => new
            {
                h.Id,
                h.Title,
                h.Description,
                h.DueDate,

                teacherAssignmentId = h.TeacherAssignmentId,

                /*
                    وقت إضافة الواجب.
                */
                h.CreatedAt,
                createdDayName = GetArabicDayName(h.CreatedAt),
                createdTime = h.CreatedAt.ToString("hh:mm tt"),

                /*
                    وقت آخر تعديل.
                    إذا لم يتم تعديل الواجب، تكون القيم فارغة.
                */
                h.UpdatedAt,
                updatedDayName = h.UpdatedAt.HasValue
                    ? GetArabicDayName(h.UpdatedAt.Value)
                    : "",
                updatedTime = h.UpdatedAt.HasValue
                    ? h.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                subjectName = h.TeacherAssignment != null &&
                              h.TeacherAssignment.Subject != null
                    ? h.TeacherAssignment.Subject.Name
                    : "",

                classRoomName = h.TeacherAssignment != null &&
                                h.TeacherAssignment.ClassRoom != null
                    ? h.TeacherAssignment.ClassRoom.Name
                    : "",

                sectionName = h.TeacherAssignment != null &&
                              h.TeacherAssignment.Section != null
                    ? h.TeacherAssignment.Section.Name
                    : ""
            }).ToList();

            return Ok(homeworks);
        }

        /*
            =====================================================
            81) API لعرض طلاب المدرس الحالي حسب التكليفات
            =====================================================

            هذا الـ endpoint يعرض الطلاب الموجودين
            في الصفوف والشعب التي يدرسها المدرس الحالي
        */
        [HttpGet("my-students")]
        public async Task<IActionResult> GetMyStudents()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var assignments = await _context.TeacherAssignments
                .Where(a => a.TeacherId == teacher.Id)
                .Select(a => new { a.ClassRoomId, a.SectionId, a.SubjectId })
                .ToListAsync();

            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .Where(s => assignments.Any(a => a.ClassRoomId == s.ClassRoomId && a.SectionId == s.SectionId))
                .Select(s => new
                {
                    s.Id,
                    s.StudentNumber,
                    fullName = s.User != null ? s.User.FullName : null,
                    username = s.User != null ? s.User.Username : null,
                    gender = s.Gender,
                    classRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null,
                    sectionName = s.Section != null ? s.Section.Name : null
                })
                .OrderBy(s => s.fullName)
                .ToListAsync();

            return Ok(students);
        }

        /*
            =====================================================
            82) API لعرض درجات الطلاب المرتبطة بمواد المدرس الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع الدرجات
            المرتبطة بتكليفات المدرس الحالي فقط
        */
        [HttpGet("my-students-grades")]
        public async Task<IActionResult> GetMyStudentsGrades()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(g => g.TeacherAssignment != null && g.TeacherAssignment.TeacherId == teacher.Id)
                .Select(g => new
                {
                    g.Id,
                    studentId = g.StudentId,
                    studentName = g.Student != null && g.Student.User != null ? g.Student.User.FullName : null,
                    studentNumber = g.Student != null ? g.Student.StudentNumber : null,
                    subjectName = g.TeacherAssignment != null && g.TeacherAssignment.Subject != null ? g.TeacherAssignment.Subject.Name : null,
                    classRoomName = g.TeacherAssignment != null && g.TeacherAssignment.ClassRoom != null ? g.TeacherAssignment.ClassRoom.Name : null,
                    sectionName = g.TeacherAssignment != null && g.TeacherAssignment.Section != null ? g.TeacherAssignment.Section.Name : null,
                    g.Part,
                    g.GradeType,
                    g.Score,
                    g.MaxScore,
                    g.Notes
                })
                .OrderBy(g => g.studentName)
                .ThenBy(g => g.subjectName)
                .ThenBy(g => g.Part)
                .ThenBy(g => g.GradeType)
                .ToListAsync();

            return Ok(grades);
        }

        /*
            =====================================================
            83) API لعرض حضور وغياب الطلاب المرتبط بمواد المدرس الحالي
            =====================================================

            هذا الـ endpoint يعرض جميع سجلات الحضور والغياب
            المرتبطة بتكليفات المدرس الحالي فقط
        */
        [HttpGet("my-students-attendances")]
        public async Task<IActionResult> GetMyStudentsAttendances()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment)
                    .ThenInclude(t => t.Section)
                .Where(a => a.TeacherAssignment != null && a.TeacherAssignment.TeacherId == teacher.Id)
                .Select(a => new
                {
                    a.Id,
                    studentId = a.StudentId,
                    studentName = a.Student != null && a.Student.User != null ? a.Student.User.FullName : null,
                    studentNumber = a.Student != null ? a.Student.StudentNumber : null,
                    subjectName = a.TeacherAssignment != null && a.TeacherAssignment.Subject != null ? a.TeacherAssignment.Subject.Name : null,
                    classRoomName = a.TeacherAssignment != null && a.TeacherAssignment.ClassRoom != null ? a.TeacherAssignment.ClassRoom.Name : null,
                    sectionName = a.TeacherAssignment != null && a.TeacherAssignment.Section != null ? a.TeacherAssignment.Section.Name : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return Ok(attendances);
        }


        /*
    =====================================================
    84) API لإضافة درس بواسطة المدرس الحالي
    =====================================================

    هذا الـ endpoint يسمح للمدرس بإضافة درس
    لكن فقط داخل تكليفاته هو
*/
        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson(CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await GetCurrentTeacherFromToken();

            if (teacher == null)
            {
                return Unauthorized("تعذر معرفة الأستاذ من التوكن.");
            }

            /*
                نتحقق أن التكليف المختار يخص هذا الأستاذ.
                حتى لا يستطيع أستاذ إضافة درس لتكليف أستاذ آخر.

                هنا نستخدم Include حتى نرجع اسم المادة والصف والشعبة بعد الإضافة.
            */
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("لا يمكنك إضافة درس لهذا التكليف.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الدرس مطلوب.");
            }

            var lesson = new Lesson
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                LessonDate = dto.LessonDate,
                CreatedAt = DateTime.Now
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الدرس بنجاح.",
                lessonId = lesson.Id,
                lessonTitle = lesson.Title,
                createdAt = lesson.CreatedAt,
                createdDayName = GetArabicDayName(lesson.CreatedAt),
                createdTime = lesson.CreatedAt.ToString("hh:mm tt"),
                subjectName = assignment.Subject != null ? assignment.Subject.Name : "",
                classRoomName = assignment.ClassRoom != null ? assignment.ClassRoom.Name : "",
                sectionName = assignment.Section != null ? assignment.Section.Name : ""
            });
        }


        /*
    =====================================================
    85) API لإضافة واجب بواسطة المدرس الحالي
    =====================================================

    هذا الـ endpoint يسمح للمدرس بإضافة واجب
    لكن فقط داخل تكليفاته هو
*/
        [HttpPost("create-homework")]
        public async Task<IActionResult> CreateHomework(CreateHomeworkDto dto)
        {
            /*
                =====================================================
                API إضافة واجب من حساب الأستاذ
                =====================================================

                هذا الأكشن يسمح للأستاذ بإضافة واجب جديد
                بشرط أن يكون التكليف التدريسي تابعًا له.

                عند الإضافة نحفظ:
                - CreatedAt: وقت إضافة الواجب
                - UpdatedAt يبقى فارغًا لأنه لم يتم تعديل الواجب بعد
            */

            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            /*
                التحقق أن التكليف التدريسي يتبع هذا المدرس.
                حتى لا يستطيع الأستاذ إضافة واجب لتكليف أستاذ آخر.
            */
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الواجب مطلوب.");
            }

            var homework = new Models.Homework
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),

                /*
                    مهلة التسليم / تاريخ تسليم الواجب.
                */
                DueDate = dto.DueDate,

                /*
                    وقت إضافة الواجب الحقيقي.
                    هذا سيظهر للأستاذ والطالب في Flutter.
                */
                CreatedAt = DateTime.Now,

                /*
                    الواجب جديد، لذلك لا يوجد وقت تعديل بعد.
                */
                UpdatedAt = null
            };

            _context.Homeworks.Add(homework);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الواجب بنجاح",

                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,

                teacherAssignmentId = homework.TeacherAssignmentId,

                /*
                    بيانات وقت الإضافة.
                */
                createdAt = homework.CreatedAt,
                createdDayName = GetArabicDayName(homework.CreatedAt),
                createdTime = homework.CreatedAt.ToString("hh:mm tt"),

                /*
                    بيانات وقت التعديل تكون فارغة عند الإضافة.
                */
                updatedAt = homework.UpdatedAt,
                updatedDayName = "",
                updatedTime = "",

                subjectName = assignment.Subject != null
                    ? assignment.Subject.Name
                    : "",

                classRoomName = assignment.ClassRoom != null
                    ? assignment.ClassRoom.Name
                    : "",

                sectionName = assignment.Section != null
                    ? assignment.Section.Name
                    : ""
            });
        }


        /*
    =====================================================
    86) API لإضافة حضور أو غياب بواسطة المدرس الحالي
    =====================================================

    هذا الـ endpoint يسمح للمدرس بإضافة حضور أو غياب
    لكن فقط داخل تكليفاته هو
*/
        [HttpPost("create-attendance")]
        public async Task<IActionResult> CreateAttendance(TeacherCreateAttendanceDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            // التحقق أن التكليف التدريسي يتبع المدرس الحالي
            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            // البحث عن الطالب
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // التحقق أن الطالب ينتمي لنفس الصف والشعبة
            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");
            }

            // التحقق من عدم وجود سجل سابق في نفس التاريخ
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == student.Id &&
                    a.TeacherAssignmentId == assignment.Id &&
                    a.AttendanceDate.Date == dto.AttendanceDate.Date);

            if (existingAttendance != null)
            {
                return BadRequest("يوجد سجل حضور أو غياب لهذا الطالب في نفس التاريخ.");
            }

            var attendance = new Models.Attendance
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status.Trim(),
                Notes = dto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة سجل الحضور/الغياب بنجاح",
                attendanceId = attendance.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                attendanceDate = attendance.AttendanceDate,
                status = attendance.Status
            });
        }



        /*
    =====================================================
    87) API لإضافة درجة بواسطة المدرس الحالي
    =====================================================

    هذا الـ endpoint يسمح للمدرس بإضافة درجة
    لكن فقط داخل تكليفاته هو
*/
        [HttpPost("create-grade")]
        public async Task<IActionResult> CreateGrade(TeacherCreateGradeDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // التحقق أن الدرجة ليست أكبر من الدرجة الكلية
            if (dto.Score > dto.MaxScore)
            {
                return BadRequest("لا يمكن أن تكون الدرجة أكبر من الدرجة الكلية.");
            }

            // التحقق من الجزء الدراسي
            var allowedParts = new[] { "الأول", "الثاني" };
            if (!allowedParts.Contains(dto.Part.Trim()))
            {
                return BadRequest("الجزء الدراسي يجب أن يكون: الأول أو الثاني.");
            }

            // التحقق من نوع الدرجة
            var allowedGradeTypes = new[] { "شهري", "سلوك", "مواظبة", "نهائي" };
            if (!allowedGradeTypes.Contains(dto.GradeType.Trim()))
            {
                return BadRequest("نوع الدرجة يجب أن يكون: شهري أو سلوك أو مواظبة أو نهائي.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            // التحقق أن التكليف التدريسي يتبع المدرس الحالي
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            // البحث عن الطالب
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            // التحقق أن الطالب ينتمي لنفس الصف والشعبة
            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");
            }

            // التحقق من عدم وجود درجة مكررة
            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.Part.Trim().ToLower() == dto.Part.Trim().ToLower() &&
                    g.GradeType.Trim().ToLower() == dto.GradeType.Trim().ToLower());

            if (existingGrade != null)
            {
                return BadRequest("توجد درجة مسجلة مسبقًا لهذا الطالب في نفس الجزء ونفس نوع الدرجة.");
            }

            // التحقق من مجموع درجات الجزء
            var currentPartTotal = await _context.Grades
                .Where(g =>
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.Part.Trim().ToLower() == dto.Part.Trim().ToLower())
                .SumAsync(g => (decimal?)g.Score) ?? 0;

            if (currentPartTotal + dto.Score > 50)
            {
                return BadRequest("لا يمكن أن يتجاوز مجموع درجات الجزء 50 درجة.");
            }

            var grade = new Models.Grade
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                Part = dto.Part.Trim(),
                GradeType = dto.GradeType.Trim(),
                Score = dto.Score,
                MaxScore = dto.MaxScore,
                Notes = dto.Notes
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إضافة الدرجة بنجاح",
                gradeId = grade.Id,
                studentName = student.User != null ? student.User.FullName : null,
                studentNumber = student.StudentNumber,
                subjectName = assignment.Subject != null ? assignment.Subject.Name : null,
                part = grade.Part,
                gradeType = grade.GradeType,
                score = grade.Score,
                maxScore = grade.MaxScore
            });
        }


        /*
    =====================================================
    88) API لتعديل درس بواسطة المدرس الحالي
    =====================================================

    هذا الـ endpoint يسمح للمدرس بتعديل درس
    لكن فقط إذا كان الدرس مرتبطًا بتكليفاته هو
*/
        [HttpPut("update-lesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            // البحث عن الدرس
            var lesson = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound("الدرس غير موجود.");
            }

            // التحقق أن الدرس الحالي نفسه يتبع المدرس الحالي
            if (lesson.TeacherAssignment == null || lesson.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك تعديل هذا الدرس لأنه لا يتبعك.");
            }

            // التحقق أن التكليف الجديد يتبع المدرس الحالي
            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الدرس مطلوب.");
            }

            lesson.TeacherAssignmentId = dto.TeacherAssignmentId;
            lesson.Title = dto.Title.Trim();
            lesson.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();
            lesson.LessonDate = dto.LessonDate;

            /*
                نحفظ تاريخ ووقت آخر تعديل.
                هذا سيظهر لاحقًا للأستاذ والطالب.
            */
            lesson.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الدرس بنجاح",
                lessonId = lesson.Id,
                updatedAt = lesson.UpdatedAt
            });
        }

        /*
            =====================================================
            89) API لحذف درس بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بحذف درس
            لكن فقط إذا كان مرتبطًا بتكليفاته هو
        */
        [HttpDelete("delete-lesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var lesson = await _context.Lessons
                .Include(l => l.TeacherAssignment)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound("الدرس غير موجود.");
            }

            if (lesson.TeacherAssignment == null || lesson.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك حذف هذا الدرس لأنه لا يتبعك.");
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الدرس بنجاح",
                lessonId = id
            });
        }

        /*
            =====================================================
            90) API لتعديل واجب بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بتعديل واجب
            لكن فقط إذا كان الواجب مرتبطًا بتكليفاته هو
        */
        [HttpPut("update-homework/{id}")]
        public async Task<IActionResult> UpdateHomework(int id, UpdateHomeworkDto dto)
        {
            /*
                =====================================================
                API تعديل واجب من حساب الأستاذ
                =====================================================

                هذا الأكشن يسمح للأستاذ بتعديل واجب موجود
                بشرطين مهمين:

                1) الواجب الحالي يكون تابعًا للأستاذ.
                2) التكليف التدريسي الجديد يكون تابعًا للأستاذ أيضًا.

                عند التعديل نحفظ:
                - UpdatedAt: وقت آخر تعديل للواجب

                هذا الوقت سيظهر لاحقًا في Flutter للأستاذ والطالب.
            */

            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            /*
                نبحث عن الواجب المطلوب تعديله
                مع التكليف التدريسي المرتبط به.
            */
            var homework = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (homework == null)
            {
                return NotFound("الواجب غير موجود.");
            }

            /*
                نتأكد أن الواجب الحالي يتبع الأستاذ الحالي.
                حتى لا يستطيع أستاذ تعديل واجب أستاذ آخر.
            */
            if (homework.TeacherAssignment == null ||
                homework.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك تعديل هذا الواجب لأنه لا يتبعك.");
            }

            /*
                نتحقق أن التكليف التدريسي الجديد يتبع الأستاذ الحالي.
                لأن الأستاذ قد يغير التكليف أثناء التعديل.
            */
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("عنوان الواجب مطلوب.");
            }

            homework.TeacherAssignmentId = dto.TeacherAssignmentId;
            homework.Title = dto.Title.Trim();
            homework.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

            /*
                تحديث مهلة التسليم / تاريخ تسليم الواجب.
            */
            homework.DueDate = dto.DueDate;

            /*
                نحفظ وقت آخر تعديل.
                هذا مهم حتى يظهر في Flutter للأستاذ والطالب.
            */
            homework.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الواجب بنجاح",

                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,

                teacherAssignmentId = homework.TeacherAssignmentId,

                /*
                    بيانات وقت آخر تعديل.
                */
                updatedAt = homework.UpdatedAt,
                updatedDayName = homework.UpdatedAt.HasValue
                    ? GetArabicDayName(homework.UpdatedAt.Value)
                    : "",
                updatedTime = homework.UpdatedAt.HasValue
                    ? homework.UpdatedAt.Value.ToString("hh:mm tt")
                    : "",

                subjectName = assignment.Subject != null
                    ? assignment.Subject.Name
                    : "",

                classRoomName = assignment.ClassRoom != null
                    ? assignment.ClassRoom.Name
                    : "",

                sectionName = assignment.Section != null
                    ? assignment.Section.Name
                    : ""
            });
        }

        /*
            =====================================================
            91) API لحذف واجب بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بحذف واجب
            لكن فقط إذا كان مرتبطًا بتكليفاته هو
        */
        [HttpDelete("delete-homework/{id}")]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            /*
                =====================================================
                API حذف واجب من حساب الأستاذ
                =====================================================

                هذا الأكشن يسمح للأستاذ بحذف واجب موجود
                بشرط أن يكون الواجب تابعًا لتكليف من تكليفاته.

                الرابط:
                DELETE api/Teacher/delete-homework/{id}
            */

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            /*
                نبحث عن الواجب المطلوب حذفه
                مع التكليف التدريسي المرتبط به.
            */
            var homework = await _context.Homeworks
                .Include(h => h.TeacherAssignment)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (homework == null)
            {
                return NotFound("الواجب غير موجود.");
            }

            /*
                نتأكد أن الواجب يتبع الأستاذ الحالي.
                حتى لا يستطيع أستاذ حذف واجب أستاذ آخر.
            */
            if (homework.TeacherAssignment == null ||
                homework.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك حذف هذا الواجب لأنه لا يتبعك.");
            }

            var deletedHomeworkId = homework.Id;
            var deletedHomeworkTitle = homework.Title;

            _context.Homeworks.Remove(homework);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الواجب بنجاح",
                homeworkId = deletedHomeworkId,
                homeworkTitle = deletedHomeworkTitle
            });
        }

        /*
            =====================================================
            92) API لتعديل حضور أو غياب بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بتعديل سجل حضور أو غياب
            لكن فقط إذا كان السجل مرتبطًا بتكليفاته هو
        */
        [HttpPut("update-attendance/{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, TeacherUpdateAttendanceDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var attendance = await _context.Attendances
                .Include(a => a.TeacherAssignment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound("سجل الحضور أو الغياب غير موجود.");
            }

            if (attendance.TeacherAssignment == null || attendance.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك تعديل هذا السجل لأنه لا يتبعك.");
            }

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");
            }

            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.Id != id &&
                    a.StudentId == student.Id &&
                    a.TeacherAssignmentId == assignment.Id &&
                    a.AttendanceDate.Date == dto.AttendanceDate.Date);

            if (existingAttendance != null)
            {
                return BadRequest("يوجد سجل حضور أو غياب آخر لهذا الطالب في نفس التاريخ.");
            }

            attendance.StudentId = student.Id;
            attendance.TeacherAssignmentId = assignment.Id;
            attendance.AttendanceDate = dto.AttendanceDate;
            attendance.Status = dto.Status.Trim();
            attendance.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل سجل الحضور/الغياب بنجاح"
            });
        }

        /*
            =====================================================
            93) API لحذف حضور أو غياب بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بحذف سجل حضور أو غياب
            لكن فقط إذا كان السجل مرتبطًا بتكليفاته هو
        */
        [HttpDelete("delete-attendance/{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var attendance = await _context.Attendances
                .Include(a => a.TeacherAssignment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound("سجل الحضور أو الغياب غير موجود.");
            }

            if (attendance.TeacherAssignment == null || attendance.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك حذف هذا السجل لأنه لا يتبعك.");
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف سجل الحضور/الغياب بنجاح"
            });
        }

        /*
            =====================================================
            94) API لتعديل درجة بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بتعديل درجة
            لكن فقط إذا كانت مرتبطة بتكليفاته هو
        */
        [HttpPut("update-grade/{id}")]
        public async Task<IActionResult> UpdateGrade(int id, TeacherUpdateGradeDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Score > dto.MaxScore)
            {
                return BadRequest("لا يمكن أن تكون الدرجة أكبر من الدرجة الكلية.");
            }

            var allowedParts = new[] { "الأول", "الثاني" };
            if (!allowedParts.Contains(dto.Part.Trim()))
            {
                return BadRequest("الجزء الدراسي يجب أن يكون: الأول أو الثاني.");
            }

            var allowedGradeTypes = new[] { "شهري", "سلوك", "مواظبة", "نهائي" };
            if (!allowedGradeTypes.Contains(dto.GradeType.Trim()))
            {
                return BadRequest("نوع الدرجة يجب أن يكون: شهري أو سلوك أو مواظبة أو نهائي.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var grade = await _context.Grades
                .Include(g => g.TeacherAssignment)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound("الدرجة غير موجودة.");
            }

            if (grade.TeacherAssignment == null || grade.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك تعديل هذه الدرجة لأنها لا تتبعك.");
            }

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.TeacherAssignmentId &&
                    a.TeacherId == teacher.Id);

            if (assignment == null)
            {
                return BadRequest("هذا التكليف التدريسي لا يتبع المدرس الحالي.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());

            if (student == null)
            {
                return BadRequest("الطالب غير موجود.");
            }

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
            {
                return BadRequest("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");
            }

            var currentPartTotal = await _context.Grades
                .Where(g =>
                    g.Id != id &&
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.Part.Trim().ToLower() == dto.Part.Trim().ToLower())
                .SumAsync(g => (decimal?)g.Score) ?? 0;

            if (currentPartTotal + dto.Score > 50)
            {
                return BadRequest("لا يمكن أن يتجاوز مجموع درجات الجزء 50 درجة.");
            }

            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g =>
                    g.Id != id &&
                    g.StudentId == student.Id &&
                    g.TeacherAssignmentId == assignment.Id &&
                    g.Part.Trim().ToLower() == dto.Part.Trim().ToLower() &&
                    g.GradeType.Trim().ToLower() == dto.GradeType.Trim().ToLower());

            if (existingGrade != null)
            {
                return BadRequest("توجد درجة أخرى بنفس الجزء ونفس النوع.");
            }

            grade.StudentId = student.Id;
            grade.TeacherAssignmentId = assignment.Id;
            grade.Part = dto.Part.Trim();
            grade.GradeType = dto.GradeType.Trim();
            grade.Score = dto.Score;
            grade.MaxScore = dto.MaxScore;
            grade.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل الدرجة بنجاح"
            });
        }

        /*
            =====================================================
            95) API لحذف درجة بواسطة المدرس الحالي
            =====================================================

            هذا الـ endpoint يسمح للمدرس بحذف درجة
            لكن فقط إذا كانت مرتبطة بتكليفاته هو
        */
        [HttpDelete("delete-grade/{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == int.Parse(userId));

            if (teacher == null)
            {
                return NotFound("المدرس غير موجود.");
            }

            var grade = await _context.Grades
                .Include(g => g.TeacherAssignment)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound("الدرجة غير موجودة.");
            }

            if (grade.TeacherAssignment == null || grade.TeacherAssignment.TeacherId != teacher.Id)
            {
                return BadRequest("لا يمكنك حذف هذه الدرجة لأنها لا تتبعك.");
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف الدرجة بنجاح"
            });
        }



        /*
    =====================================================
    API عرض جدول الأستاذ الحالي
    =====================================================

    هذا API يعتمد على التوكن.

    طريقة العمل:
    1) نأخذ UserId من التوكن.
    2) نبحث عن الأستاذ المرتبط بهذا UserId.
    3) نعرض فقط الحصص التي TeacherAssignment.TeacherId يساوي Id الأستاذ.
*/
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdText) || !int.TryParse(userIdText, out int userId))
            {
                return Unauthorized("تعذر معرفة المستخدم من التوكن.");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound("الأستاذ غير موجود لهذا المستخدم.");
            }

            var schedules = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.ClassRoom)
                .Include(s => s.TeacherAssignment)
                    .ThenInclude(a => a.Section)
                .Where(s =>
                    s.TeacherAssignment != null &&
                    s.TeacherAssignment.TeacherId == teacher.Id)
                .Select(s => new TeacherScheduleDto
                {
                    Id = s.Id,
                    DayName = s.DayName,
                    PeriodNumber = s.PeriodNumber,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    RoomName = s.RoomName,
                    Notes = s.Notes,

                    SubjectName = s.TeacherAssignment!.Subject != null
                        ? s.TeacherAssignment.Subject.Name
                        : "",

                    ClassRoomName = s.TeacherAssignment.ClassRoom != null
                        ? s.TeacherAssignment.ClassRoom.Name
                        : "",

                    SectionName = s.TeacherAssignment.Section != null
                        ? s.TeacherAssignment.Section.Name
                        : ""
                })
                .ToListAsync();

            var dayOrder = new Dictionary<string, int>
    {
        { "السبت", 1 },
        { "الأحد", 2 },
        { "الاثنين", 3 },
        { "الثلاثاء", 4 },
        { "الأربعاء", 5 },
        { "الخميس", 6 }
    };

            schedules = schedules
                .OrderBy(s => dayOrder.ContainsKey(s.DayName) ? dayOrder[s.DayName] : 99)
                .ThenBy(s => s.PeriodNumber)
                .ToList();

            return Ok(schedules);
        }



        /*
    =====================================================
    جلب الأستاذ الحالي من التوكن
    =====================================================

    التوكن يحتوي على UserId.
    نبحث عن الأستاذ المرتبط بهذا المستخدم.
*/
        private async Task<Teacher?> GetCurrentTeacherFromToken()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdText) || !int.TryParse(userIdText, out int userId))
            {
                return null;
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            return teacher;
        }


        /*
    =====================================================
    تحويل اليوم إلى اللغة العربية
    =====================================================
*/
        private string GetArabicDayName(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => "السبت",
                DayOfWeek.Sunday => "الأحد",
                DayOfWeek.Monday => "الاثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الأربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                _ => ""
            };
        }


    }
}