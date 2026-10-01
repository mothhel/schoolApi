using Microsoft.EntityFrameworkCore;
using school.Application.Interfaces;
using school.Domain.Entities;
using School.Application.DTOs;

namespace school.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IApplicationDbContext _context;

        public TeacherService(IApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================================
        // دوال مساعدة (Helper Methods)
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

        private async Task<Teacher> GetTeacherByUserIdAsync(int userId)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) throw new Exception("المدرس غير موجود.");
            return teacher;
        }

        // ===============================================
        // عمليات جلب البيانات (GET)
        // ===============================================
        public async Task<object> GetMyProfileAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Teacher).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Teacher == null) throw new Exception("المدرس غير موجود.");

            return new
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
            };
        }

        public async Task<object> GetMyAssignmentsAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            return await _context.TeacherAssignments
                .Include(a => a.Subject)
                .Include(a => a.ClassRoom)
                .Include(a => a.Section)
                .Where(a => a.TeacherId == teacher.Id)
                .Select(a => new TeacherAssignmentSelectDto
                {
                    Id = a.Id,
                    SubjectName = a.Subject != null ? a.Subject.Name : "",
                    ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : "",
                    SectionName = a.Section != null ? a.Section.Name : "",
                    DisplayName = (a.Subject != null ? a.Subject.Name : "") + " - " +
                                  (a.ClassRoom != null ? a.ClassRoom.Name : "") + " - " +
                                  (a.Section != null ? a.Section.Name : "")
                })
                .OrderBy(a => a.ClassRoomName).ThenBy(a => a.SectionName).ThenBy(a => a.SubjectName)
                .ToListAsync();
        }

        public async Task<object> GetMyLessonsAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var lessonsFromDb = await _context.Lessons
                .Include(l => l.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(l => l.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(l => l.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(l => l.TeacherAssignment != null && l.TeacherAssignment.TeacherId == teacher.Id)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            return lessonsFromDb.Select(l => new TeacherLessonDto
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
                SubjectName = l.TeacherAssignment?.Subject?.Name ?? "",
                ClassRoomName = l.TeacherAssignment?.ClassRoom?.Name ?? "",
                SectionName = l.TeacherAssignment?.Section?.Name ?? ""
            }).ToList();
        }

        public async Task<object> GetMyHomeworksAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var homeworksFromDb = await _context.Homeworks
                .Include(h => h.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(h => h.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(h => h.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(h => h.TeacherAssignment != null && h.TeacherAssignment.TeacherId == teacher.Id)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            return homeworksFromDb.Select(h => new
            {
                h.Id,
                h.Title,
                h.Description,
                h.DueDate,
                teacherAssignmentId = h.TeacherAssignmentId,
                h.CreatedAt,
                createdDayName = GetArabicDayName(h.CreatedAt),
                createdTime = h.CreatedAt.ToString("hh:mm tt"),
                h.UpdatedAt,
                updatedDayName = h.UpdatedAt.HasValue ? GetArabicDayName(h.UpdatedAt.Value) : "",
                updatedTime = h.UpdatedAt.HasValue ? h.UpdatedAt.Value.ToString("hh:mm tt") : "",
                subjectName = h.TeacherAssignment?.Subject?.Name ?? "",
                classRoomName = h.TeacherAssignment?.ClassRoom?.Name ?? "",
                sectionName = h.TeacherAssignment?.Section?.Name ?? ""
            }).ToList();
        }

        public async Task<object> GetMyStudentsAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var assignments = await _context.TeacherAssignments
                .Where(a => a.TeacherId == teacher.Id)
                .Select(a => new { a.ClassRoomId, a.SectionId })
                .ToListAsync();

            return await _context.Students
                .Include(s => s.User).Include(s => s.ClassRoom).Include(s => s.Section)
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
        }

        public async Task<object> GetMyStudentsGradesAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            return await _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(g => g.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(g => g.TeacherAssignment != null && g.TeacherAssignment.TeacherId == teacher.Id)
                .Select(g => new
                {
                    g.Id,
                    studentId = g.StudentId,
                    studentName = g.Student.User != null ? g.Student.User.FullName : null,
                    studentNumber = g.Student.StudentNumber,
                    subjectName = g.TeacherAssignment.Subject != null ? g.TeacherAssignment.Subject.Name : null,
                    classRoomName = g.TeacherAssignment.ClassRoom != null ? g.TeacherAssignment.ClassRoom.Name : null,
                    sectionName = g.TeacherAssignment.Section != null ? g.TeacherAssignment.Section.Name : null,
                    g.Part,
                    g.GradeType,
                    g.Score,
                    g.MaxScore,
                    g.Notes
                })
                .OrderBy(g => g.studentName).ThenBy(g => g.subjectName).ThenBy(g => g.Part).ThenBy(g => g.GradeType)
                .ToListAsync();
        }

        public async Task<object> GetMyStudentsAttendancesAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            return await _context.Attendances
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.TeacherAssignment).ThenInclude(t => t.Subject)
                .Include(a => a.TeacherAssignment).ThenInclude(t => t.ClassRoom)
                .Include(a => a.TeacherAssignment).ThenInclude(t => t.Section)
                .Where(a => a.TeacherAssignment != null && a.TeacherAssignment.TeacherId == teacher.Id)
                .Select(a => new
                {
                    a.Id,
                    studentId = a.StudentId,
                    studentName = a.Student.User != null ? a.Student.User.FullName : null,
                    studentNumber = a.Student.StudentNumber,
                    subjectName = a.TeacherAssignment.Subject != null ? a.TeacherAssignment.Subject.Name : null,
                    classRoomName = a.TeacherAssignment.ClassRoom != null ? a.TeacherAssignment.ClassRoom.Name : null,
                    sectionName = a.TeacherAssignment.Section != null ? a.TeacherAssignment.Section.Name : null,
                    a.AttendanceDate,
                    a.Status,
                    a.Notes
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<object> GetMyScheduleAsync(int userId)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var schedules = await _context.SchoolSchedules
                .Include(s => s.TeacherAssignment).ThenInclude(a => a.Subject)
                .Include(s => s.TeacherAssignment).ThenInclude(a => a.ClassRoom)
                .Include(s => s.TeacherAssignment).ThenInclude(a => a.Section)
                .Where(s => s.TeacherAssignment != null && s.TeacherAssignment.TeacherId == teacher.Id)
                .Select(s => new TeacherScheduleDto
                {
                    Id = s.Id,
                    DayName = s.DayName,
                    PeriodNumber = s.PeriodNumber,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    RoomName = s.RoomName,
                    Notes = s.Notes,
                    SubjectName = s.TeacherAssignment!.Subject != null ? s.TeacherAssignment.Subject.Name : "",
                    ClassRoomName = s.TeacherAssignment.ClassRoom != null ? s.TeacherAssignment.ClassRoom.Name : "",
                    SectionName = s.TeacherAssignment.Section != null ? s.TeacherAssignment.Section.Name : ""
                })
                .ToListAsync();

            var dayOrder = new Dictionary<string, int> { { "السبت", 1 }, { "الأحد", 2 }, { "الاثنين", 3 }, { "الثلاثاء", 4 }, { "الأربعاء", 5 }, { "الخميس", 6 } };

            return schedules.OrderBy(s => dayOrder.ContainsKey(s.DayName) ? dayOrder[s.DayName] : 99).ThenBy(s => s.PeriodNumber).ToList();
        }

        // ===============================================
        // عمليات الإضافة (Create)
        // ===============================================
        public async Task<object> CreateLessonAsync(int userId, CreateLessonDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);

            if (assignment == null) throw new Exception("لا يمكنك إضافة درس لهذا التكليف.");

            var lesson = new Lesson
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                LessonDate = dto.LessonDate,
                CreatedAt = DateTime.Now
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return new
            {
                message = "تم إضافة الدرس بنجاح.",
                lessonId = lesson.Id,
                lessonTitle = lesson.Title,
                createdAt = lesson.CreatedAt,
                createdDayName = GetArabicDayName(lesson.CreatedAt),
                createdTime = lesson.CreatedAt.ToString("hh:mm tt"),
                subjectName = assignment.Subject?.Name ?? "",
                classRoomName = assignment.ClassRoom?.Name ?? "",
                sectionName = assignment.Section?.Name ?? ""
            };
        }

        public async Task<object> CreateHomeworkAsync(int userId, CreateHomeworkDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var assignment = await _context.TeacherAssignments
                .Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);

            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            var homework = new Homework
            {
                TeacherAssignmentId = dto.TeacherAssignmentId,
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                DueDate = dto.DueDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            _context.Homeworks.Add(homework);
            await _context.SaveChangesAsync();

            return new
            {
                message = "تم إضافة الواجب بنجاح",
                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,
                teacherAssignmentId = homework.TeacherAssignmentId,
                createdAt = homework.CreatedAt,
                createdDayName = GetArabicDayName(homework.CreatedAt),
                createdTime = homework.CreatedAt.ToString("hh:mm tt"),
                updatedAt = homework.UpdatedAt,
                updatedDayName = "",
                updatedTime = "",
                subjectName = assignment.Subject?.Name ?? "",
                classRoomName = assignment.ClassRoom?.Name ?? "",
                sectionName = assignment.Section?.Name ?? ""
            };
        }

        public async Task<object> CreateAttendanceAsync(int userId, TeacherCreateAttendanceDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);

            var assignment = await _context.TeacherAssignments
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);

            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());
            if (student == null) throw new Exception("الطالب غير موجود.");

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
                throw new Exception("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");

            var existing = await _context.Attendances.FirstOrDefaultAsync(a => a.StudentId == student.Id && a.TeacherAssignmentId == assignment.Id && a.AttendanceDate.Date == dto.AttendanceDate.Date);
            if (existing != null) throw new Exception("يوجد سجل حضور أو غياب لهذا الطالب في نفس التاريخ.");

            var attendance = new Attendance
            {
                StudentId = student.Id,
                TeacherAssignmentId = assignment.Id,
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status.Trim(),
                Notes = dto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return new
            {
                message = "تم إضافة سجل الحضور/الغياب بنجاح",
                attendanceId = attendance.Id,
                studentName = student.User?.FullName,
                studentNumber = student.StudentNumber,
                attendanceDate = attendance.AttendanceDate,
                status = attendance.Status
            };
        }

        public async Task<object> CreateGradeAsync(int userId, TeacherCreateGradeDto dto)
        {
            if (dto.Score > dto.MaxScore) throw new Exception("لا يمكن أن تكون الدرجة أكبر من الدرجة الكلية.");

            var allowedParts = new[] { "الأول", "الثاني" };
            if (!allowedParts.Contains(dto.Part.Trim())) throw new Exception("الجزء الدراسي يجب أن يكون: الأول أو الثاني.");

            var allowedGradeTypes = new[] { "شهري", "سلوك", "مواظبة", "نهائي" };
            if (!allowedGradeTypes.Contains(dto.GradeType.Trim())) throw new Exception("نوع الدرجة يجب أن يكون: شهري أو سلوك أو مواظبة أو نهائي.");

            var teacher = await GetTeacherByUserIdAsync(userId);

            var assignment = await _context.TeacherAssignments.Include(a => a.Subject).FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());
            if (student == null) throw new Exception("الطالب غير موجود.");

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId)
                throw new Exception("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");

            var existing = await _context.Grades.FirstOrDefaultAsync(g => g.StudentId == student.Id && g.TeacherAssignmentId == assignment.Id && g.Part.Trim().ToLower() == dto.Part.Trim().ToLower() && g.GradeType.Trim().ToLower() == dto.GradeType.Trim().ToLower());
            if (existing != null) throw new Exception("توجد درجة مسجلة مسبقًا لهذا الطالب في نفس الجزء ونفس نوع الدرجة.");

            var currentPartTotal = await _context.Grades.Where(g => g.StudentId == student.Id && g.TeacherAssignmentId == assignment.Id && g.Part.Trim().ToLower() == dto.Part.Trim().ToLower()).SumAsync(g => (decimal?)g.Score) ?? 0;
            if (currentPartTotal + dto.Score > 50) throw new Exception("لا يمكن أن يتجاوز مجموع درجات الجزء 50 درجة.");

            var grade = new Grade
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

            return new
            {
                message = "تم إضافة الدرجة بنجاح",
                gradeId = grade.Id,
                studentName = student.User?.FullName,
                studentNumber = student.StudentNumber,
                subjectName = assignment.Subject?.Name,
                part = grade.Part,
                gradeType = grade.GradeType,
                score = grade.Score,
                maxScore = grade.MaxScore
            };
        }

        // ===============================================
        // عمليات التعديل (Update)
        // ===============================================
        public async Task<object> UpdateLessonAsync(int userId, int id, UpdateLessonDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var lesson = await _context.Lessons.Include(l => l.TeacherAssignment).FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) throw new Exception("الدرس غير موجود.");
            if (lesson.TeacherAssignment == null || lesson.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك تعديل هذا الدرس لأنه لا يتبعك.");

            var assignment = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            lesson.TeacherAssignmentId = dto.TeacherAssignmentId;
            lesson.Title = dto.Title.Trim();
            lesson.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            lesson.LessonDate = dto.LessonDate;
            lesson.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new { message = "تم تعديل الدرس بنجاح", lessonId = lesson.Id, updatedAt = lesson.UpdatedAt };
        }

        public async Task<object> UpdateHomeworkAsync(int userId, int id, UpdateHomeworkDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var homework = await _context.Homeworks.Include(h => h.TeacherAssignment).FirstOrDefaultAsync(h => h.Id == id);

            if (homework == null) throw new Exception("الواجب غير موجود.");
            if (homework.TeacherAssignment == null || homework.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك تعديل هذا الواجب لأنه لا يتبعك.");

            var assignment = await _context.TeacherAssignments.Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section)
                .FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            homework.TeacherAssignmentId = dto.TeacherAssignmentId;
            homework.Title = dto.Title.Trim();
            homework.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            homework.DueDate = dto.DueDate;
            homework.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new
            {
                message = "تم تعديل الواجب بنجاح",
                homeworkId = homework.Id,
                homeworkTitle = homework.Title,
                description = homework.Description,
                dueDate = homework.DueDate,
                teacherAssignmentId = homework.TeacherAssignmentId,
                updatedAt = homework.UpdatedAt,
                updatedDayName = homework.UpdatedAt.HasValue ? GetArabicDayName(homework.UpdatedAt.Value) : "",
                updatedTime = homework.UpdatedAt.HasValue ? homework.UpdatedAt.Value.ToString("hh:mm tt") : "",
                subjectName = assignment.Subject?.Name ?? "",
                classRoomName = assignment.ClassRoom?.Name ?? "",
                sectionName = assignment.Section?.Name ?? ""
            };
        }

        public async Task<object> UpdateAttendanceAsync(int userId, int id, TeacherUpdateAttendanceDto dto)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var attendance = await _context.Attendances.Include(a => a.TeacherAssignment).FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null) throw new Exception("سجل الحضور أو الغياب غير موجود.");
            if (attendance.TeacherAssignment == null || attendance.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك تعديل هذا السجل لأنه لا يتبعك.");

            var assignment = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());
            if (student == null) throw new Exception("الطالب غير موجود.");

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId) throw new Exception("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");

            var existing = await _context.Attendances.FirstOrDefaultAsync(a => a.Id != id && a.StudentId == student.Id && a.TeacherAssignmentId == assignment.Id && a.AttendanceDate.Date == dto.AttendanceDate.Date);
            if (existing != null) throw new Exception("يوجد سجل حضور أو غياب آخر لهذا الطالب في نفس التاريخ.");

            attendance.StudentId = student.Id;
            attendance.TeacherAssignmentId = assignment.Id;
            attendance.AttendanceDate = dto.AttendanceDate;
            attendance.Status = dto.Status.Trim();
            attendance.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return new { message = "تم تعديل سجل الحضور/الغياب بنجاح" };
        }

        public async Task<object> UpdateGradeAsync(int userId, int id, TeacherUpdateGradeDto dto)
        {
            if (dto.Score > dto.MaxScore) throw new Exception("لا يمكن أن تكون الدرجة أكبر من الدرجة الكلية.");

            var allowedParts = new[] { "الأول", "الثاني" };
            if (!allowedParts.Contains(dto.Part.Trim())) throw new Exception("الجزء الدراسي يجب أن يكون: الأول أو الثاني.");

            var allowedGradeTypes = new[] { "شهري", "سلوك", "مواظبة", "نهائي" };
            if (!allowedGradeTypes.Contains(dto.GradeType.Trim())) throw new Exception("نوع الدرجة يجب أن يكون: شهري أو سلوك أو مواظبة أو نهائي.");

            var teacher = await GetTeacherByUserIdAsync(userId);
            var grade = await _context.Grades.Include(g => g.TeacherAssignment).FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null) throw new Exception("الدرجة غير موجودة.");
            if (grade.TeacherAssignment == null || grade.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك تعديل هذه الدرجة لأنها لا تتبعك.");

            var assignment = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) throw new Exception("هذا التكليف التدريسي لا يتبع المدرس الحالي.");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower());
            if (student == null) throw new Exception("الطالب غير موجود.");

            if (student.ClassRoomId != assignment.ClassRoomId || student.SectionId != assignment.SectionId) throw new Exception("هذا الطالب لا ينتمي إلى الصف أو الشعبة الخاصة بهذا التكليف.");

            var currentPartTotal = await _context.Grades.Where(g => g.Id != id && g.StudentId == student.Id && g.TeacherAssignmentId == assignment.Id && g.Part.Trim().ToLower() == dto.Part.Trim().ToLower()).SumAsync(g => (decimal?)g.Score) ?? 0;
            if (currentPartTotal + dto.Score > 50) throw new Exception("لا يمكن أن يتجاوز مجموع درجات الجزء 50 درجة.");

            var existingGrade = await _context.Grades.FirstOrDefaultAsync(g => g.Id != id && g.StudentId == student.Id && g.TeacherAssignmentId == assignment.Id && g.Part.Trim().ToLower() == dto.Part.Trim().ToLower() && g.GradeType.Trim().ToLower() == dto.GradeType.Trim().ToLower());
            if (existingGrade != null) throw new Exception("توجد درجة أخرى بنفس الجزء ونفس النوع.");

            grade.StudentId = student.Id;
            grade.TeacherAssignmentId = assignment.Id;
            grade.Part = dto.Part.Trim();
            grade.GradeType = dto.GradeType.Trim();
            grade.Score = dto.Score;
            grade.MaxScore = dto.MaxScore;
            grade.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return new { message = "تم تعديل الدرجة بنجاح" };
        }

        // ===============================================
        // عمليات الحذف (Delete)
        // ===============================================
        public async Task<object> DeleteLessonAsync(int userId, int id)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var lesson = await _context.Lessons.Include(l => l.TeacherAssignment).FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) throw new Exception("الدرس غير موجود.");
            if (lesson.TeacherAssignment == null || lesson.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك حذف هذا الدرس لأنه لا يتبعك.");

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return new { message = "تم حذف الدرس بنجاح", lessonId = id };
        }

        public async Task<object> DeleteHomeworkAsync(int userId, int id)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var homework = await _context.Homeworks.Include(h => h.TeacherAssignment).FirstOrDefaultAsync(h => h.Id == id);

            if (homework == null) throw new Exception("الواجب غير موجود.");
            if (homework.TeacherAssignment == null || homework.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك حذف هذا الواجب لأنه لا يتبعك.");

            var deletedHomeworkId = homework.Id;
            var deletedHomeworkTitle = homework.Title;

            _context.Homeworks.Remove(homework);
            await _context.SaveChangesAsync();
            return new { message = "تم حذف الواجب بنجاح", homeworkId = deletedHomeworkId, homeworkTitle = deletedHomeworkTitle };
        }

        public async Task<object> DeleteAttendanceAsync(int userId, int id)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var attendance = await _context.Attendances.Include(a => a.TeacherAssignment).FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null) throw new Exception("سجل الحضور أو الغياب غير موجود.");
            if (attendance.TeacherAssignment == null || attendance.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك حذف هذا السجل لأنه لا يتبعك.");

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return new { message = "تم حذف سجل الحضور/الغياب بنجاح" };
        }

        public async Task<object> DeleteGradeAsync(int userId, int id)
        {
            var teacher = await GetTeacherByUserIdAsync(userId);
            var grade = await _context.Grades.Include(g => g.TeacherAssignment).FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null) throw new Exception("الدرجة غير موجودة.");
            if (grade.TeacherAssignment == null || grade.TeacherAssignment.TeacherId != teacher.Id) throw new Exception("لا يمكنك حذف هذه الدرجة لأنها لا تتبعك.");

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return new { message = "تم حذف الدرجة بنجاح" };
        }
    }
}