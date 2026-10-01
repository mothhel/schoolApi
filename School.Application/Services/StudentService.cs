using Microsoft.EntityFrameworkCore;
using school.Application.Interfaces;
using school.Domain.Entities;
using School.Application.DTOs;

namespace school.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IApplicationDbContext _context;

        public StudentService(IApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================================
        // دوال مساعدة (Helper Methods) كانت في الكنترولر
        // ===============================================
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

        private bool IsMatchingAssessment(Grade grade, string assessmentType)
        {
            var part = grade.Part?.Trim().ToLower() ?? "";
            var gradeType = grade.GradeType?.Trim().ToLower() ?? "";
            var termName = grade.TermName?.Trim().ToLower() ?? "";
            var combined = $"{part} {gradeType} {termName}";

            if (assessmentType == "First")
            {
                return combined.Contains("first") || combined.Contains("1") || combined.Contains("الأولى") ||
                       combined.Contains("الاولى") || combined.Contains("المحصلة الأولى") || combined.Contains("المحصله الاولى") ||
                       combined.Contains("محصلة أولى") || combined.Contains("محصله اولى");
            }
            if (assessmentType == "Second")
            {
                return combined.Contains("second") || combined.Contains("2") || combined.Contains("الثانية") ||
                       combined.Contains("الثانيه") || combined.Contains("المحصلة الثانية") || combined.Contains("المحصله الثانيه") ||
                       combined.Contains("محصلة ثانية") || combined.Contains("محصله ثانيه");
            }
            return false;
        }

        private string? GetGradeComponentKey(Grade grade)
        {
            var part = grade.Part?.Trim().ToLower() ?? "";
            var gradeType = grade.GradeType?.Trim().ToLower() ?? "";
            var termName = grade.TermName?.Trim().ToLower() ?? "";
            var text = $"{part} {gradeType} {termName}";

            var isAssessment = text.Contains("محصلة") || text.Contains("محصله");

            if (text.Contains("المحصلة الأولى") || text.Contains("المحصله الاولى") || text.Contains("محصلة أولى") ||
                text.Contains("محصله اولى") || text.Contains("first assessment") || text.Contains("first"))
                return "FirstAssessment";

            if (text.Contains("المحصلة الثانية") || text.Contains("المحصله الثانيه") || text.Contains("محصلة ثانية") ||
                text.Contains("محصله ثانيه") || text.Contains("second assessment") || text.Contains("second"))
                return "SecondAssessment";

            if (!isAssessment && (text.Contains("نتيجة نصف العام") || text.Contains("نصف العام") ||
                                  text.Contains("اختبار نصف العام") || text.Contains("midyear") || text.Contains("mid year")))
                return "MidYearResult";

            if (!isAssessment && (text.Contains("نتيجة نهاية العام") || text.Contains("نهاية العام") ||
                                  text.Contains("اختبار نهاية العام") || text.Contains("finalyear") || text.Contains("final year")))
                return "FinalYearResult";

            return null;
        }

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

        private List<string> GetRequiredResultComponents(string resultType)
        {
            if (resultType == "MidYear")
                return new List<string> { "FirstAssessment", "MidYearResult" };

            return new List<string> { "FirstAssessment", "MidYearResult", "SecondAssessment", "FinalYearResult" };
        }

        // ===============================================
        // تنفيذ العمليات الأساسية
        // ===============================================

        public async Task<object> GetMyProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Student).ThenInclude(s => s.ClassRoom)
                .Include(u => u.Student).ThenInclude(s => s.Section)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Student == null)
                throw new Exception("الطالب غير موجود.");

            return new
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
                classRoomName = user.Student.ClassRoom?.Name,
                sectionId = user.Student.SectionId,
                sectionName = user.Student.Section?.Name
            };
        }

        public async Task<object> GetMySubjectsAsync(int userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            return await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.Teacher).ThenInclude(t => t.User)
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
        }

        public async Task<object> GetMyGradesAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User)
                .Where(g => g.StudentId == student.Id)
                .Select(g => new
                {
                    g.Id,
                    subjectName = g.TeacherAssignment != null && g.TeacherAssignment.Subject != null ? g.TeacherAssignment.Subject.Name : null,
                    teacherName = g.TeacherAssignment != null && g.TeacherAssignment.Teacher != null && g.TeacherAssignment.Teacher.User != null
                        ? g.TeacherAssignment.Teacher.User.FullName : null,
                    g.Part,
                    g.GradeType,
                    g.Score,
                    g.MaxScore,
                    g.Notes
                })
                .OrderBy(g => g.subjectName).ThenBy(g => g.Part).ThenBy(g => g.GradeType)
                .ToListAsync();

            return new { studentId = student.Id, studentName = student.User?.FullName, studentNumber = student.StudentNumber, grades };
        }

        public async Task<object> GetMyAttendancesAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var attendances = await _context.Attendances
                .Include(a => a.TeacherAssignment).ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment).ThenInclude(t => t.Teacher).ThenInclude(t => t.User)
                .Where(a => a.StudentId == student.Id)
                .Select(a => new
                {
                    a.Id,
                    subjectName = a.TeacherAssignment != null && a.TeacherAssignment.Subject != null ? a.TeacherAssignment.Subject.Name : null,
                    teacherName = a.TeacherAssignment != null && a.TeacherAssignment.Teacher != null && a.TeacherAssignment.Teacher.User != null
                        ? a.TeacherAssignment.Teacher.User.FullName : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return new { studentId = student.Id, studentName = student.User?.FullName, studentNumber = student.StudentNumber, attendances };
        }

        public async Task<object> GetMyNotesAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var notes = await _context.StudentNotes
                .Where(n => n.StudentId == student.Id)
                .Select(n => new { n.Id, n.NoteText, n.CreatedAt })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return new { studentId = student.Id, studentName = student.User?.FullName, studentNumber = student.StudentNumber, notes };
        }

        public async Task<object> GetMyDailyTasksAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var tasks = await _context.DailyTasks
                .Where(t => t.StudentId == student.Id)
                .Select(t => new { t.Id, t.Title, t.Description, t.TaskDate, t.IsCompleted })
                .OrderByDescending(t => t.TaskDate)
                .ToListAsync();

            return new { studentId = student.Id, studentName = student.User?.FullName, studentNumber = student.StudentNumber, dailyTasks = tasks };
        }

        public async Task<object> MarkMyAreaAsViewedAsync(int userId, string areaKey)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            int maxId = 0;
            switch (areaKey)
            {
                case "Lessons":
                    maxId = await _context.Lessons.Where(l => l.TeacherAssignment.ClassRoomId == student.ClassRoomId && l.TeacherAssignment.SectionId == student.SectionId).Select(l => l.Id).DefaultIfEmpty().MaxAsync();
                    break;
                case "Homeworks":
                    maxId = await _context.Homeworks.Where(h => h.TeacherAssignment.ClassRoomId == student.ClassRoomId && h.TeacherAssignment.SectionId == student.SectionId).Select(h => h.Id).DefaultIfEmpty().MaxAsync();
                    break;
                case "Attendance":
                    maxId = await _context.Attendances.Where(a => a.StudentId == student.Id).Select(a => a.Id).DefaultIfEmpty().MaxAsync();
                    break;
                case "Schedule":
                    maxId = await _context.SchoolSchedules.Where(s => s.TeacherAssignment.ClassRoomId == student.ClassRoomId && s.TeacherAssignment.SectionId == student.SectionId).Select(s => s.Id).DefaultIfEmpty().MaxAsync();
                    break;
                case "Notes":
                    maxId = await _context.StudentNotes.Where(n => n.StudentId == student.Id).Select(n => n.Id).DefaultIfEmpty().MaxAsync();
                    break;
                default:
                    throw new Exception("نوع القسم غير صحيح.");
            }

            var status = await _context.StudentPortalReadStatuses.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.AreaKey == areaKey);
            if (status == null)
            {
                status = new StudentPortalReadStatus { StudentId = student.Id, AreaKey = areaKey, LastSeenId = maxId, LastViewedAt = DateTime.Now };
                _context.StudentPortalReadStatuses.Add(status);
            }
            else
            {
                status.LastSeenId = maxId;
                status.LastViewedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return new { message = "تم تحديث حالة المشاهدة بنجاح.", studentId = student.Id, areaKey, lastSeenId = maxId };
        }

        public async Task<object> GetMyNewCountsAsync(int userId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            // دالة جلب آخر قراءة
            int GetLastSeen(string areaKey)
            {
                return _context.StudentPortalReadStatuses
                    .Where(x => x.StudentId == student.Id && x.AreaKey == areaKey)
                    .Select(x => x.LastSeenId)
                    .FirstOrDefault();
            }

            // 1. يجب جلب الأرقام وتخزينها في متغيرات أولاً قبل استعلام EF Core
            var lessonsLastSeen = GetLastSeen("Lessons");
            var homeworksLastSeen = GetLastSeen("Homeworks");
            var attendanceLastSeen = GetLastSeen("Attendance");
            var scheduleLastSeen = GetLastSeen("Schedule");
            var notesLastSeen = GetLastSeen("Notes");

            // 2. الآن نستخدم المتغيرات الرقمية بأمان داخل الاستعلامات
            return new
            {
                LessonsCount = await _context.Lessons
                    .Where(l => l.Id > lessonsLastSeen &&
                                l.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                                l.TeacherAssignment.SectionId == student.SectionId)
                    .CountAsync(),

                HomeworksCount = await _context.Homeworks
                    .Where(h => h.Id > homeworksLastSeen &&
                                h.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                                h.TeacherAssignment.SectionId == student.SectionId)
                    .CountAsync(),

                AttendanceCount = await _context.Attendances
                    .Where(a => a.Id > attendanceLastSeen &&
                                a.StudentId == student.Id)
                    .CountAsync(),

                ScheduleCount = await _context.SchoolSchedules
                    .Where(s => s.Id > scheduleLastSeen &&
                                s.TeacherAssignment.ClassRoomId == student.ClassRoomId &&
                                s.TeacherAssignment.SectionId == student.SectionId)
                    .CountAsync(),

                NotesCount = await _context.StudentNotes
                    .Where(n => n.Id > notesLastSeen &&
                                n.StudentId == student.Id)
                    .CountAsync()
            };
        }

        public async Task<object> GetMyGradeClassRoomsAsync(int userId)
        {
            var student = await _context.Students.Include(s => s.ClassRoom).Include(s => s.Section).FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var classRooms = await _context.Grades
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(g => g.StudentId == student.Id && g.TeacherAssignment != null)
                .Select(g => new StudentGradeClassRoomDto
                {
                    ClassRoomId = g.TeacherAssignment!.ClassRoomId,
                    ClassRoomName = g.TeacherAssignment.ClassRoom != null ? g.TeacherAssignment.ClassRoom.Name : "",
                    SectionId = g.TeacherAssignment.SectionId,
                    SectionName = g.TeacherAssignment.Section != null ? g.TeacherAssignment.Section.Name : ""
                })
                .Distinct().OrderBy(x => x.ClassRoomName).ToListAsync();

            if (!classRooms.Any())
            {
                classRooms.Add(new StudentGradeClassRoomDto
                {
                    ClassRoomId = student.ClassRoomId,
                    ClassRoomName = student.ClassRoom?.Name ?? "",
                    SectionId = student.SectionId,
                    SectionName = student.Section?.Name ?? ""
                });
            }
            return classRooms;
        }

        public async Task<object> GetMyAssessmentGradesAsync(int userId, int classRoomId, string assessmentType)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var gradesQuery = await _context.Grades
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(g => g.StudentId == student.Id && g.TeacherAssignment != null && g.TeacherAssignment.ClassRoomId == classRoomId)
                .ToListAsync();

            var grades = gradesQuery.Where(g => IsMatchingAssessment(g, assessmentType)).ToList();
            var firstGrade = grades.FirstOrDefault();

            if (firstGrade == null)
            {
                var classInfo = await _context.TeacherAssignments.Include(a => a.ClassRoom).Include(a => a.Section).Where(a => a.ClassRoomId == classRoomId).FirstOrDefaultAsync();
                return new StudentAssessmentGradesDto
                {
                    ClassRoomId = classRoomId,
                    ClassRoomName = classInfo?.ClassRoom?.Name ?? "",
                    SectionId = classInfo?.SectionId ?? 0,
                    SectionName = classInfo?.Section?.Name ?? "",
                    AssessmentType = assessmentType,
                    AssessmentTitle = assessmentType == "First" ? "المحصلة الأولى" : "المحصلة الثانية",
                    Items = new List<StudentAssessmentGradeItemDto>()
                };
            }

            var items = grades.Where(g => g.TeacherAssignment != null).GroupBy(g => g.TeacherAssignment!.SubjectId)
                .Select(group =>
                {
                    var grade = group.OrderByDescending(g => g.Id).First();
                    return new StudentAssessmentGradeItemDto
                    {
                        SubjectName = grade.TeacherAssignment?.Subject?.Name ?? "",
                        TeacherName = grade.TeacherAssignment?.Teacher?.User?.FullName ?? "",
                        Score = grade.Score,
                        MaxScore = grade.MaxScore > 0 ? grade.MaxScore : 20,
                        Notes = grade.Notes
                    };
                }).OrderBy(x => x.SubjectName).ToList();

            return new StudentAssessmentGradesDto
            {
                ClassRoomId = firstGrade.TeacherAssignment!.ClassRoomId,
                ClassRoomName = firstGrade.TeacherAssignment.ClassRoom?.Name ?? "",
                SectionId = firstGrade.TeacherAssignment.SectionId,
                SectionName = firstGrade.TeacherAssignment.Section?.Name ?? "",
                AssessmentType = assessmentType,
                AssessmentTitle = assessmentType == "First" ? "المحصلة الأولى" : "المحصلة الثانية",
                Items = items
            };
        }

        public async Task<object> GetMyClassResultAsync(int userId, int classRoomId, string resultType)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) throw new Exception("الطالب غير موجود.");

            var requiredComponents = GetRequiredResultComponents(resultType);
            var grades = await _context.Grades
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(g => g.StudentId == student.Id && g.TeacherAssignment != null && g.TeacherAssignment.ClassRoomId == classRoomId)
                .ToListAsync();

            if (!grades.Any())
            {
                return new StudentClassResultDto
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
                };
            }

            var firstGrade = grades.First();
            var subjects = grades.Where(g => g.TeacherAssignment != null).GroupBy(g => g.TeacherAssignment!.SubjectId)
                .Select(group =>
                {
                    var subjectGrades = group.ToList();
                    var anyGrade = subjectGrades.OrderByDescending(g => g.Id).First();
                    var components = requiredComponents.Select(key =>
                    {
                        var grade = subjectGrades.Where(g => GetGradeComponentKey(g) == key).OrderByDescending(g => g.Id).FirstOrDefault();
                        return new StudentResultComponentDto { Key = key, Title = GetComponentTitle(key), Score = grade?.Score ?? 0, MaxScore = grade != null && grade.MaxScore > 0 ? grade.MaxScore : 20 };
                    }).ToList();

                    var tScore = components.Sum(x => x.Score);
                    var tMax = components.Sum(x => x.MaxScore);
                    var isComplete = requiredComponents.All(k => subjectGrades.Any(g => GetGradeComponentKey(g) == k));

                    return new StudentResultSubjectDto
                    {
                        SubjectName = anyGrade.TeacherAssignment?.Subject?.Name ?? "",
                        TeacherName = anyGrade.TeacherAssignment?.Teacher?.User?.FullName ?? "",
                        Components = components,
                        TotalScore = tScore,
                        TotalMaxScore = tMax,
                        Percentage = tMax > 0 ? Math.Round((tScore / tMax) * 100, 2) : 0,
                        Status = !isComplete ? "غير مكتمل" : (tMax > 0 && (tScore / tMax) >= 0.5m ? "ناجح" : "راسب")
                    };
                }).OrderBy(x => x.SubjectName).ToList();

            var totalScoreAll = subjects.Sum(x => x.TotalScore);
            var totalMaxAll = subjects.Sum(x => x.TotalMaxScore);
            var totalPercentage = totalMaxAll > 0 ? Math.Round((totalScoreAll / totalMaxAll) * 100, 2) : 0;

            return new StudentClassResultDto
            {
                ClassRoomId = firstGrade.TeacherAssignment!.ClassRoomId,
                ClassRoomName = firstGrade.TeacherAssignment.ClassRoom?.Name ?? "",
                SectionId = firstGrade.TeacherAssignment.SectionId,
                SectionName = firstGrade.TeacherAssignment.Section?.Name ?? "",
                ResultType = resultType,
                ResultTitle = resultType == "MidYear" ? "نتيجة نصف العام" : "نتيجة نهاية العام",
                TotalScore = totalScoreAll,
                TotalMaxScore = totalMaxAll,
                Percentage = totalPercentage,
                OverallStatus = subjects.Any(x => x.Status == "غير مكتمل") ? "غير مكتمل" : (totalPercentage >= 50 ? "ناجح" : "راسب"),
                Subjects = subjects
            };
        }
    }
}