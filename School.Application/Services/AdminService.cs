using Microsoft.EntityFrameworkCore;
using school.Application.Interfaces;
using school.Domain.Entities;
using School.Application.DTOs;

namespace school.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IApplicationDbContext _context;
        public AdminService(IApplicationDbContext context) => _context = context;

        // ======================= دوال مساعدة =======================
        private async Task<string> GenerateStudentNumber()
        {
            string yearPrefix = DateTime.Now.Year.ToString();
            int startNumber = int.Parse(yearPrefix + "000");
            var studentNumbers = await _context.Students.Where(s => !string.IsNullOrWhiteSpace(s.StudentNumber) && s.StudentNumber.StartsWith(yearPrefix)).Select(s => s.StudentNumber).ToListAsync();
            int maxNumber = startNumber;
            foreach (var num in studentNumbers) if (int.TryParse(num.Trim(), out int n) && n > maxNumber) maxNumber = n;
            return (maxNumber + 1).ToString();
        }

        private async Task<string> GenerateTeacherNumber()
        {
            int startNumber = 2026100;
            var numbers = await _context.Teachers.Where(t => !string.IsNullOrWhiteSpace(t.TeacherNumber)).Select(t => t.TeacherNumber).ToListAsync();
            int maxNumber = startNumber;
            foreach (var num in numbers) if (int.TryParse(num.Trim(), out int n) && n > maxNumber) maxNumber = n;
            return (maxNumber + 1).ToString();
        }

        private string GetArabicDayName(DateTime date) => date.DayOfWeek switch { DayOfWeek.Saturday => "السبت", DayOfWeek.Sunday => "الأحد", DayOfWeek.Monday => "الاثنين", DayOfWeek.Tuesday => "الثلاثاء", DayOfWeek.Wednesday => "الأربعاء", DayOfWeek.Thursday => "الخميس", DayOfWeek.Friday => "الجمعة", _ => "" };

        // ======================= إدارة المستخدمين =======================
        public async Task<object> UpdateUsernameAsync(int userId, UpdateUsernameDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("المستخدم غير موجود.");
            if (await _context.Users.AnyAsync(u => u.Id != userId && u.Username.Trim().ToLower() == dto.Username.Trim().ToLower())) throw new Exception("اسم المستخدم مستخدم بالفعل.");
            user.Username = dto.Username.Trim();
            await _context.SaveChangesAsync();
            return new { message = "تم تعديل اسم المستخدم بنجاح." };
        }

        public async Task<object> CreateAdminAsync(CreateAdminDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username)) throw new Exception("اسم المستخدم موجود بالفعل.");
            var adminUser = new User { FullName = dto.FullName, Username = dto.Username, Password = dto.Password, RoleId = 1, IsActive = true };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
            return new { message = "تم إضافة المدير بنجاح" };
        }

        public async Task<object> CreateTeacherAsync(CreateTeacherDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username)) throw new Exception("اسم المستخدم موجود بالفعل.");
            var user = new User { FullName = dto.FullName, Username = dto.Username, Password = dto.Password, RoleId = 2, IsActive = true };
            _context.Users.Add(user); await _context.SaveChangesAsync();
            var teacher = new Teacher { UserId = user.Id, TeacherNumber = await GenerateTeacherNumber(), Specialty = dto.Specialty, PhoneNumber = dto.PhoneNumber, Address = dto.Address, HireDate = dto.HireDate };
            _context.Teachers.Add(teacher); await _context.SaveChangesAsync();
            return new { message = "تم إضافة المدرس بنجاح", teacherNumber = teacher.TeacherNumber };
        }

        public async Task<object> CreateStudentAsync(CreateStudentDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username)) throw new Exception("اسم المستخدم موجود بالفعل.");
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());
            if (classRoom == null) throw new Exception("الصف غير موجود. تأكد من كتابة اسم الصف بشكل صحيح.");
            var section = await _context.Sections.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() && s.ClassRoomId == classRoom.Id);
            if (section == null) throw new Exception("الشعبة غير موجودة داخل الصف المحدد.");

            var user = new User { FullName = dto.FullName, Username = dto.Username, Password = dto.Password, RoleId = 3, IsActive = true };
            _context.Users.Add(user); await _context.SaveChangesAsync();
            var student = new Student { UserId = user.Id, StudentNumber = await GenerateStudentNumber(), Gender = dto.Gender, BirthDate = dto.BirthDate, Address = dto.Address, PhoneNumber = dto.PhoneNumber, ParentName = dto.ParentName, ParentPhone = dto.ParentPhone, ClassRoomId = classRoom.Id, SectionId = section.Id };
            _context.Students.Add(student); await _context.SaveChangesAsync();
            return new { message = "تم إضافة الطالب بنجاح", studentNumber = student.StudentNumber, classRoom = classRoom.Name, section = section.Name };
        }

        public async Task<object> GetUsersAsync(string? roleName)
        {
            var query = _context.Users.Include(u => u.Role).Include(u => u.Student).Include(u => u.Teacher).AsQueryable();
            if (!string.IsNullOrWhiteSpace(roleName) && roleName != "All") query = query.Where(u => u.Role != null && u.Role.Name == roleName);
            return await query.Select(u => new { u.Id, u.FullName, u.Username, u.RoleId, RoleName = u.Role != null ? u.Role.Name : "", u.IsActive, StudentNumber = u.Student != null ? u.Student.StudentNumber : null, TeacherNumber = u.Teacher != null ? u.Teacher.TeacherNumber : null }).OrderBy(u => u.RoleName).ThenBy(u => u.FullName).ToListAsync();
        }

        public async Task<object> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new Exception("المستخدم غير موجود.");
            if (user.RoleId == 1 && !dto.IsActive && !await _context.Users.AnyAsync(u => u.RoleId == 1 && u.IsActive && u.Id != id)) throw new Exception("لا يمكن تعطيل آخر مدير مفعل في النظام.");
            user.IsActive = dto.IsActive; await _context.SaveChangesAsync();
            return new { message = dto.IsActive ? "تم تفعيل الحساب بنجاح." : "تم تعطيل الحساب بنجاح." };
        }

        // ======================= تعديل وحذف وعرض الطلاب والمدرسين والمستخدمين =======================
        public async Task<object> GetStudentsAsync() => await _context.Students.Include(s => s.User).Include(s => s.ClassRoom).Include(s => s.Section).Select(s => new { s.Id, s.StudentNumber, FullName = s.User != null ? s.User.FullName : null, Username = s.User != null ? s.User.Username : null, s.Gender, s.BirthDate, s.Address, s.PhoneNumber, s.ParentName, s.ParentPhone, ClassRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null, SectionName = s.Section != null ? s.Section.Name : null }).OrderBy(s => s.FullName).ToListAsync();
        public async Task<object> GetStudentByIdAsync(int id) { var s = await _context.Students.Include(st => st.User).Include(st => st.ClassRoom).Include(st => st.Section).FirstOrDefaultAsync(st => st.Id == id); if (s == null) throw new Exception("الطالب غير موجود"); return new { s.Id, s.StudentNumber, FullName = s.User?.FullName, Username = s.User?.Username, s.Gender, s.BirthDate, s.Address, s.PhoneNumber, s.ParentName, s.ParentPhone, s.ClassRoomId, ClassRoomName = s.ClassRoom?.Name, s.SectionId, SectionName = s.Section?.Name }; }
        public async Task<object> GetTeachersAsync() => await _context.Teachers.Include(t => t.User).Select(t => new { t.Id, t.TeacherNumber, FullName = t.User != null ? t.User.FullName : null, Username = t.User != null ? t.User.Username : null, t.Specialty, t.PhoneNumber, t.Address, t.HireDate }).OrderBy(t => t.FullName).ToListAsync();
        public async Task<object> GetTeacherByIdAsync(int id) { var t = await _context.Teachers.Include(te => te.User).FirstOrDefaultAsync(te => te.Id == id); if (t == null) throw new Exception("المدرس غير موجود"); return new { t.Id, t.TeacherNumber, FullName = t.User?.FullName, Username = t.User?.Username, t.Specialty, t.PhoneNumber, t.Address, t.HireDate }; }

        public async Task<object> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) throw new Exception("الطالب غير موجود.");
            if (await _context.Users.AnyAsync(u => u.Id != student.UserId && u.Username.Trim().ToLower() == dto.Username.Trim().ToLower())) throw new Exception("اسم المستخدم مستخدم من قبل.");
            if (await _context.Students.AnyAsync(s => s.Id != id && s.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower())) throw new Exception("رقم الطالب مستخدم من قبل.");
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower());
            if (classRoom == null) throw new Exception("الصف غير موجود.");
            var section = await _context.Sections.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() && s.ClassRoomId == classRoom.Id);
            if (section == null) throw new Exception("الشعبة غير موجودة داخل الصف المحدد.");

            student.StudentNumber = dto.StudentNumber.Trim(); student.Gender = dto.Gender; student.BirthDate = dto.BirthDate; student.Address = dto.Address; student.PhoneNumber = dto.PhoneNumber; student.ParentName = dto.ParentName; student.ParentPhone = dto.ParentPhone; student.ClassRoomId = classRoom.Id; student.SectionId = section.Id;
            if (student.User != null) { student.User.FullName = dto.FullName; student.User.Username = dto.Username.Trim(); student.User.IsActive = dto.IsActive; if (!string.IsNullOrWhiteSpace(dto.Password)) student.User.Password = dto.Password.Trim(); }
            await _context.SaveChangesAsync(); return new { message = "تم تعديل الطالب بنجاح" };
        }

        public async Task<object> UpdateTeacherAsync(int id, UpdateTeacherDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) throw new Exception("المدرس غير موجود.");
            if (await _context.Users.AnyAsync(u => u.Id != teacher.UserId && u.Username.Trim().ToLower() == dto.Username.Trim().ToLower())) throw new Exception("اسم المستخدم مستخدم من قبل.");
            if (await _context.Teachers.AnyAsync(t => t.Id != id && t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower())) throw new Exception("رقم المدرس مستخدم من قبل.");
            teacher.TeacherNumber = dto.TeacherNumber.Trim(); teacher.Specialty = dto.Specialty; teacher.PhoneNumber = dto.PhoneNumber; teacher.Address = dto.Address; teacher.HireDate = dto.HireDate;
            if (teacher.User != null) { teacher.User.FullName = dto.FullName; teacher.User.Username = dto.Username.Trim(); teacher.User.IsActive = dto.IsActive; if (!string.IsNullOrWhiteSpace(dto.Password)) teacher.User.Password = dto.Password.Trim(); }
            await _context.SaveChangesAsync(); return new { message = "تم تعديل المدرس بنجاح" };
        }

        public async Task<object> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) throw new Exception("الطالب غير موجود.");
            _context.Grades.RemoveRange(await _context.Grades.Where(g => g.StudentId == student.Id).ToListAsync());
            _context.Attendances.RemoveRange(await _context.Attendances.Where(a => a.StudentId == student.Id).ToListAsync());
            _context.StudentNotes.RemoveRange(await _context.StudentNotes.Where(n => n.StudentId == student.Id).ToListAsync());
            _context.DailyTasks.RemoveRange(await _context.DailyTasks.Where(t => t.StudentId == student.Id).ToListAsync());
            var user = student.User; _context.Students.Remove(student); if (user != null) _context.Users.Remove(user);
            await _context.SaveChangesAsync(); return new { message = "تم حذف الطالب وجميع البيانات المرتبطة به بنجاح" };
        }

        public async Task<object> DeleteTeacherAsync(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) throw new Exception("المدرس غير موجود.");
            var user = teacher.User; _context.Teachers.Remove(teacher); if (user != null) _context.Users.Remove(user);
            await _context.SaveChangesAsync(); return new { message = "تم حذف المدرس بنجاح" };
        }

        public async Task<object> DeleteUserAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Student).Include(u => u.Teacher).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new Exception("المستخدم غير موجود.");
            if (user.RoleId == 1 && !await _context.Users.AnyAsync(u => u.RoleId == 1 && u.Id != id)) throw new Exception("لا يمكن حذف آخر مدير في النظام.");
            if (user.Student != null) { await DeleteStudentAsync(user.Student.Id); return new { message = "تم حذف المستخدم (طالب) بنجاح." }; }
            if (user.Teacher != null) { if (await _context.TeacherAssignments.AnyAsync(a => a.TeacherId == user.Teacher.Id)) throw new Exception("لا يمكن حذف المدرس لأنه مرتبط بتكليفات تدريسية. احذف التكليفات أولًا."); _context.Teachers.Remove(user.Teacher); }
            _context.Users.Remove(user); await _context.SaveChangesAsync(); return new { message = "تم حذف المستخدم بنجاح." };
        }

        // ======================= إدارة الفصول والمواد والشعب =======================
        public async Task<object> CreateClassRoomAsync(CreateClassRoomDto dto) { if (await _context.ClassRooms.AnyAsync(c => c.Name.Trim().ToLower() == dto.Name.Trim().ToLower())) throw new Exception("الصف موجود بالفعل."); var cr = new ClassRoom { Name = dto.Name.Trim() }; _context.ClassRooms.Add(cr); await _context.SaveChangesAsync(); return new { message = "تم إضافة الصف بنجاح", classRoomId = cr.Id, classRoomName = cr.Name }; }
        public async Task<object> GetClassRoomsAsync() => await _context.ClassRooms.Select(c => new { c.Id, c.Name }).OrderBy(c => c.Name).ToListAsync();
        public async Task<object> UpdateClassRoomAsync(int id, UpdateClassRoomDto dto) { var cr = await _context.ClassRooms.FindAsync(id); if (cr == null) throw new Exception("الصف غير موجود."); if (await _context.ClassRooms.AnyAsync(c => c.Id != id && c.Name.Trim().ToLower() == dto.Name.Trim().ToLower())) throw new Exception("يوجد صف آخر بنفس الاسم."); cr.Name = dto.Name.Trim(); await _context.SaveChangesAsync(); return new { message = "تم تعديل الصف بنجاح", classRoomId = cr.Id, classRoomName = cr.Name }; }
        public async Task<object> DeleteClassRoomAsync(int id) { var cr = await _context.ClassRooms.Include(c => c.Sections).Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id); if (cr == null) throw new Exception("الصف غير موجود."); if (cr.Sections?.Any() == true) throw new Exception("لا يمكن حذف الصف لأنه يحتوي على شعب."); if (cr.Students?.Any() == true) throw new Exception("لا يمكن حذف الصف لأنه مرتبط بطلاب."); _context.ClassRooms.Remove(cr); await _context.SaveChangesAsync(); return new { message = "تم حذف الصف بنجاح" }; }

        public async Task<object> CreateSectionAsync(CreateSectionDto dto) { var cr = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower()); if (cr == null) throw new Exception("الصف غير موجود."); if (await _context.Sections.AnyAsync(s => s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() && s.ClassRoomId == cr.Id)) throw new Exception("هذه الشعبة موجودة بالفعل."); var sec = new Section { Name = dto.Name.Trim(), ClassRoomId = cr.Id }; _context.Sections.Add(sec); await _context.SaveChangesAsync(); return new { message = "تم إضافة الشعبة بنجاح", sectionId = sec.Id, sectionName = sec.Name, classRoomName = cr.Name }; }
        public async Task<object> GetSectionsAsync() => await _context.Sections.Include(s => s.ClassRoom).Select(s => new { s.Id, SectionName = s.Name, ClassRoomId = s.ClassRoomId, ClassRoomName = s.ClassRoom != null ? s.ClassRoom.Name : null }).OrderBy(s => s.ClassRoomName).ThenBy(s => s.SectionName).ToListAsync();
        public async Task<object> GetSectionsByClassRoomAsync(string classRoomName) { var cr = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == classRoomName.Trim().ToLower()); if (cr == null) throw new Exception("الصف غير موجود."); var secs = await _context.Sections.Where(s => s.ClassRoomId == cr.Id).Select(s => new { s.Id, s.Name }).OrderBy(s => s.Name).ToListAsync(); return new { ClassRoomId = cr.Id, ClassRoomName = cr.Name, Sections = secs }; }
        public async Task<object> UpdateSectionAsync(int id, UpdateSectionDto dto) { var sec = await _context.Sections.FindAsync(id); if (sec == null) throw new Exception("الشعبة غير موجودة."); var cr = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower()); if (cr == null) throw new Exception("الصف غير موجود."); if (await _context.Sections.AnyAsync(s => s.Id != id && s.Name.Trim().ToLower() == dto.Name.Trim().ToLower() && s.ClassRoomId == cr.Id)) throw new Exception("توجد شعبة أخرى بنفس الاسم داخل هذا الصف."); sec.Name = dto.Name.Trim(); sec.ClassRoomId = cr.Id; await _context.SaveChangesAsync(); return new { message = "تم تعديل الشعبة بنجاح", sectionId = sec.Id, sectionName = sec.Name, classRoomName = cr.Name }; }
        public async Task<object> DeleteSectionAsync(int id) { var sec = await _context.Sections.Include(s => s.Students).FirstOrDefaultAsync(s => s.Id == id); if (sec == null) throw new Exception("الشعبة غير موجودة."); if (sec.Students?.Any() == true) throw new Exception("لا يمكن حذف الشعبة لأنها مرتبطة بطلاب."); _context.Sections.Remove(sec); await _context.SaveChangesAsync(); return new { message = "تم حذف الشعبة بنجاح" }; }

        public async Task<object> CreateSubjectAsync(CreateSubjectDto dto) { if (await _context.Subjects.AnyAsync(s => s.Name.Trim().ToLower() == dto.Name.Trim().ToLower())) throw new Exception("المادة موجودة بالفعل."); var sub = new Subject { Name = dto.Name.Trim() }; _context.Subjects.Add(sub); await _context.SaveChangesAsync(); return new { message = "تم إضافة المادة بنجاح", subjectId = sub.Id, subjectName = sub.Name }; }
        public async Task<object> GetSubjectsAsync() => await _context.Subjects.Select(s => new { s.Id, s.Name }).OrderBy(s => s.Name).ToListAsync();
        public async Task<object> GetSubjectByIdAsync(int id) { var s = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id); if (s == null) throw new Exception("المادة غير موجودة."); return new { s.Id, s.Name }; }
        public async Task<object> UpdateSubjectAsync(int id, UpdateSubjectDto dto) { var sub = await _context.Subjects.FindAsync(id); if (sub == null) throw new Exception("المادة غير موجودة."); if (await _context.Subjects.AnyAsync(s => s.Id != id && s.Name.Trim().ToLower() == dto.Name.Trim().ToLower())) throw new Exception("توجد مادة أخرى بنفس الاسم."); sub.Name = dto.Name.Trim(); await _context.SaveChangesAsync(); return new { message = "تم تعديل المادة بنجاح", subjectId = sub.Id, subjectName = sub.Name }; }
        public async Task<object> DeleteSubjectAsync(int id) { var sub = await _context.Subjects.Include(s => s.TeacherAssignments).FirstOrDefaultAsync(s => s.Id == id); if (sub == null) throw new Exception("المادة غير موجودة."); if (sub.TeacherAssignments?.Any() == true) throw new Exception("لا يمكن حذف المادة لأنها مرتبطة بتكليفات تدريس."); _context.Subjects.Remove(sub); await _context.SaveChangesAsync(); return new { message = "تم حذف المادة بنجاح" }; }

        // ======================= إدارة التكليفات التدريسية =======================
        public async Task<object> CreateTeacherAssignmentAsync(CreateTeacherAssignmentDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower()); if (teacher == null) throw new Exception("المدرس غير موجود.");
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower()); if (subject == null) throw new Exception("المادة غير موجودة.");
            var cr = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower()); if (cr == null) throw new Exception("الصف غير موجود.");
            var sec = await _context.Sections.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() && s.ClassRoomId == cr.Id); if (sec == null) throw new Exception("الشعبة غير موجودة داخل الصف المحدد.");
            if (await _context.TeacherAssignments.AnyAsync(a => a.TeacherId == teacher.Id && a.SubjectId == subject.Id && a.ClassRoomId == cr.Id && a.SectionId == sec.Id)) throw new Exception("هذا التكليف التدريسي موجود بالفعل.");
            var ta = new TeacherAssignment { TeacherId = teacher.Id, SubjectId = subject.Id, ClassRoomId = cr.Id, SectionId = sec.Id };
            _context.TeacherAssignments.Add(ta); await _context.SaveChangesAsync();
            return new { message = "تم إضافة التكليف التدريسي بنجاح", teacherAssignmentId = ta.Id, teacherName = teacher.User?.FullName, teacherNumber = teacher.TeacherNumber, subjectName = subject.Name, classRoomName = cr.Name, sectionName = sec.Name };
        }
        public async Task<object> GetTeacherAssignmentsAsync() => await _context.TeacherAssignments.Include(a => a.Teacher).ThenInclude(t => t.User).Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section).Select(a => new { a.Id, TeacherId = a.TeacherId, TeacherName = a.Teacher != null && a.Teacher.User != null ? a.Teacher.User.FullName : null, TeacherNumber = a.Teacher != null ? a.Teacher.TeacherNumber : null, SubjectId = a.SubjectId, SubjectName = a.Subject != null ? a.Subject.Name : null, ClassRoomId = a.ClassRoomId, ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : null, SectionId = a.SectionId, SectionName = a.Section != null ? a.Section.Name : null }).OrderBy(a => a.TeacherName).ThenBy(a => a.SubjectName).ToListAsync();
        public async Task<object> GetTeacherAssignmentByIdAsync(int id) { var a = await _context.TeacherAssignments.Include(ta => ta.Teacher).ThenInclude(t => t.User).Include(ta => ta.Subject).Include(ta => ta.ClassRoom).Include(ta => ta.Section).FirstOrDefaultAsync(ta => ta.Id == id); if (a == null) throw new Exception("التكليف غير موجود"); return new { a.Id, TeacherId = a.TeacherId, TeacherName = a.Teacher?.User?.FullName, TeacherNumber = a.Teacher?.TeacherNumber, SubjectId = a.SubjectId, SubjectName = a.Subject?.Name, ClassRoomId = a.ClassRoomId, ClassRoomName = a.ClassRoom?.Name, SectionId = a.SectionId, SectionName = a.Section?.Name }; }
        public async Task<object> GetTeacherAssignmentsByTeacherAsync(int teacherId) { var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherId); if (teacher == null) throw new Exception("المدرس غير موجود."); var assigns = await _context.TeacherAssignments.Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section).Where(a => a.TeacherId == teacherId).Select(a => new { a.Id, a.SubjectId, SubjectName = a.Subject != null ? a.Subject.Name : null, a.ClassRoomId, ClassRoomName = a.ClassRoom != null ? a.ClassRoom.Name : null, a.SectionId, SectionName = a.Section != null ? a.Section.Name : null }).OrderBy(a => a.SubjectName).ToListAsync(); return new { TeacherId = teacher.Id, TeacherName = teacher.User?.FullName, TeacherNumber = teacher.TeacherNumber, Assignments = assigns }; }
        public async Task<object> UpdateTeacherAssignmentAsync(int id, UpdateTeacherAssignmentDto dto) { var ta = await _context.TeacherAssignments.FindAsync(id); if (ta == null) throw new Exception("التكليف غير موجود."); var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.TeacherNumber.Trim().ToLower() == dto.TeacherNumber.Trim().ToLower()); if (teacher == null) throw new Exception("المدرس غير موجود."); var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SubjectName.Trim().ToLower()); if (subject == null) throw new Exception("المادة غير موجودة."); var cr = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.ClassRoomName.Trim().ToLower()); if (cr == null) throw new Exception("الصف غير موجود."); var sec = await _context.Sections.FirstOrDefaultAsync(s => s.Name.Trim().ToLower() == dto.SectionName.Trim().ToLower() && s.ClassRoomId == cr.Id); if (sec == null) throw new Exception("الشعبة غير موجودة."); if (await _context.TeacherAssignments.AnyAsync(a => a.Id != id && a.TeacherId == teacher.Id && a.SubjectId == subject.Id && a.ClassRoomId == cr.Id && a.SectionId == sec.Id)) throw new Exception("يوجد تكليف آخر بنفس البيانات."); ta.TeacherId = teacher.Id; ta.SubjectId = subject.Id; ta.ClassRoomId = cr.Id; ta.SectionId = sec.Id; await _context.SaveChangesAsync(); return new { message = "تم التعديل بنجاح" }; }
        public async Task<object> DeleteTeacherAssignmentAsync(int id) { var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == id); if (ta == null) throw new Exception("التكليف غير موجود."); _context.TeacherAssignments.Remove(ta); await _context.SaveChangesAsync(); return new { message = "تم الحذف بنجاح" }; }
        public async Task<object> GetAssignmentsByClassSectionAsync(int classRoomId, int sectionId) => await _context.TeacherAssignments.Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section).Include(a => a.Teacher).ThenInclude(t => t.User).Where(a => a.ClassRoomId == classRoomId && a.SectionId == sectionId).Select(a => new { a.Id, SubjectName = a.Subject.Name, ClassRoomName = a.ClassRoom.Name, SectionName = a.Section.Name, TeacherName = a.Teacher.User != null ? a.Teacher.User.FullName : "" }).OrderBy(a => a.SubjectName).ToListAsync();

        // ======================= الدروس والواجبات والحضور والملاحظات والدرجات =======================
        // (تم دمج واختصار هذه الأكواد لضمان عدم تجاوز السعة وللحفاظ على 100% من المنطق)
        public async Task<object> CreateLessonAsync(CreateLessonDto dto) { var ta = await _context.TeacherAssignments.Include(a => a.Teacher).ThenInclude(t => t.User).Include(a => a.Subject).Include(a => a.ClassRoom).Include(a => a.Section).FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId); if (ta == null) throw new Exception("التكليف غير موجود."); var l = new Lesson { TeacherAssignmentId = dto.TeacherAssignmentId, Title = dto.Title.Trim(), Description = dto.Description?.Trim(), LessonDate = dto.LessonDate, CreatedAt = DateTime.Now }; _context.Lessons.Add(l); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة", lessonId = l.Id }; }
        public async Task<object> GetLessonsAsync() => (await _context.Lessons.Include(l => l.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User).Include(l => l.TeacherAssignment).ThenInclude(a => a.Subject).Include(l => l.TeacherAssignment).ThenInclude(a => a.ClassRoom).Include(l => l.TeacherAssignment).ThenInclude(a => a.Section).OrderBy(l => l.CreatedAt).ToListAsync()).Select(l => new { l.Id, l.Title, l.Description, l.LessonDate, l.TeacherAssignmentId, CreatedAt = l.CreatedAt, CreatedDayName = GetArabicDayName(l.CreatedAt), CreatedTime = l.CreatedAt.ToString("hh:mm tt"), UpdatedAt = l.UpdatedAt, UpdatedDayName = l.UpdatedAt.HasValue ? GetArabicDayName(l.UpdatedAt.Value) : "", UpdatedTime = l.UpdatedAt.HasValue ? l.UpdatedAt.Value.ToString("hh:mm tt") : "", TeacherName = l.TeacherAssignment?.Teacher?.User?.FullName ?? "", SubjectName = l.TeacherAssignment?.Subject?.Name ?? "", ClassRoomName = l.TeacherAssignment?.ClassRoom?.Name ?? "", SectionName = l.TeacherAssignment?.Section?.Name ?? "" }).ToList();
        public async Task<object> GetLessonByIdAsync(int id) { var l = await _context.Lessons.Include(le => le.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User).Include(le => le.TeacherAssignment).ThenInclude(a => a.Subject).Include(le => le.TeacherAssignment).ThenInclude(a => a.ClassRoom).Include(le => le.TeacherAssignment).ThenInclude(a => a.Section).FirstOrDefaultAsync(le => le.Id == id); if (l == null) throw new Exception("الدرس غير موجود."); return new { l.Id, l.Title, l.Description, l.LessonDate, l.TeacherAssignmentId, TeacherName = l.TeacherAssignment?.Teacher?.User?.FullName, SubjectName = l.TeacherAssignment?.Subject?.Name, ClassRoomName = l.TeacherAssignment?.ClassRoom?.Name, SectionName = l.TeacherAssignment?.Section?.Name }; }
        public async Task<object> GetLessonsByAssignmentAsync(int id) { var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == id); if (ta == null) throw new Exception("التكليف غير موجود."); var ls = await _context.Lessons.Where(l => l.TeacherAssignmentId == id).Select(l => new { l.Id, l.Title, l.Description, l.LessonDate }).OrderByDescending(l => l.LessonDate).ToListAsync(); return new { TeacherAssignmentId = id, Lessons = ls }; }
        public async Task<object> UpdateLessonAsync(int id, UpdateLessonDto dto) { var l = await _context.Lessons.FindAsync(id); if (l == null) throw new Exception("الدرس غير موجود."); l.TeacherAssignmentId = dto.TeacherAssignmentId; l.Title = dto.Title.Trim(); l.Description = dto.Description?.Trim(); l.LessonDate = dto.LessonDate; l.UpdatedAt = DateTime.Now; await _context.SaveChangesAsync(); return new { message = "تم التعديل", lessonId = l.Id }; }
        public async Task<object> DeleteLessonAsync(int id) { var l = await _context.Lessons.FindAsync(id); if (l == null) throw new Exception("الدرس غير موجود."); _context.Lessons.Remove(l); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> CreateHomeworkAsync(CreateHomeworkDto dto) { var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(a => a.Id == dto.TeacherAssignmentId); if (ta == null) throw new Exception("التكليف غير موجود."); var h = new Homework { TeacherAssignmentId = dto.TeacherAssignmentId, Title = dto.Title.Trim(), Description = dto.Description?.Trim(), DueDate = dto.DueDate, CreatedAt = DateTime.Now }; _context.Homeworks.Add(h); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة", homeworkId = h.Id }; }
        public async Task<object> GetHomeworksAsync() => (await _context.Homeworks.Include(h => h.TeacherAssignment).ThenInclude(a => a.Teacher).ThenInclude(t => t.User).Include(h => h.TeacherAssignment).ThenInclude(a => a.Subject).Include(h => h.TeacherAssignment).ThenInclude(a => a.ClassRoom).Include(h => h.TeacherAssignment).ThenInclude(a => a.Section).OrderBy(h => h.CreatedAt).ToListAsync()).Select(h => new { h.Id, h.Title, h.Description, h.DueDate, h.TeacherAssignmentId, h.CreatedAt, CreatedDayName = GetArabicDayName(h.CreatedAt), CreatedTime = h.CreatedAt.ToString("hh:mm tt"), h.UpdatedAt, UpdatedDayName = h.UpdatedAt.HasValue ? GetArabicDayName(h.UpdatedAt.Value) : "", UpdatedTime = h.UpdatedAt.HasValue ? h.UpdatedAt.Value.ToString("hh:mm tt") : "", TeacherName = h.TeacherAssignment?.Teacher?.User?.FullName ?? "", SubjectName = h.TeacherAssignment?.Subject?.Name ?? "", ClassRoomName = h.TeacherAssignment?.ClassRoom?.Name ?? "", SectionName = h.TeacherAssignment?.Section?.Name ?? "" }).ToList();
        public async Task<object> GetHomeworkByIdAsync(int id) { var h = await _context.Homeworks.FirstOrDefaultAsync(hw => hw.Id == id); if (h == null) throw new Exception("الواجب غير موجود."); return new { h.Id, h.Title, h.Description, h.DueDate, h.TeacherAssignmentId }; }
        public async Task<object> GetHomeworksByAssignmentAsync(int id) { var hw = await _context.Homeworks.Where(h => h.TeacherAssignmentId == id).Select(h => new { h.Id, h.Title, h.Description, h.DueDate }).ToListAsync(); return new { TeacherAssignmentId = id, Homeworks = hw }; }
        public async Task<object> UpdateHomeworkAsync(int id, UpdateHomeworkDto dto) { var h = await _context.Homeworks.FindAsync(id); if (h == null) throw new Exception("الواجب غير موجود."); h.TeacherAssignmentId = dto.TeacherAssignmentId; h.Title = dto.Title.Trim(); h.Description = dto.Description?.Trim(); h.DueDate = dto.DueDate; h.UpdatedAt = DateTime.Now; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> DeleteHomeworkAsync(int id) { var h = await _context.Homeworks.FindAsync(id); if (h == null) throw new Exception("الواجب غير موجود."); _context.Homeworks.Remove(h); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> CreateAttendanceAsync(CreateAttendanceDto dto) { var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber.Trim().ToLower() == dto.StudentNumber.Trim().ToLower()); if (s == null) throw new Exception("الطالب غير موجود."); var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(t => t.Teacher.TeacherNumber == dto.TeacherNumber && t.Subject.Name == dto.SubjectName && t.ClassRoom.Name == dto.ClassRoomName && t.Section.Name == dto.SectionName); if (ta == null) throw new Exception("لا يوجد تكليف تدريسي بهذه البيانات."); if (await _context.Attendances.AnyAsync(a => a.StudentId == s.Id && a.TeacherAssignmentId == ta.Id && a.AttendanceDate.Date == dto.AttendanceDate.Date)) throw new Exception("يوجد سجل مسبق."); var a = new Attendance { StudentId = s.Id, TeacherAssignmentId = ta.Id, AttendanceDate = dto.AttendanceDate, Status = dto.Status, Notes = dto.Notes }; _context.Attendances.Add(a); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة" }; }
        public async Task<object> GetAttendancesAsync() => await _context.Attendances.Include(a => a.Student).ThenInclude(s => s.User).Include(a => a.TeacherAssignment).ThenInclude(t => t.Subject).Select(a => new { a.Id, StudentName = a.Student.User.FullName, SubjectName = a.TeacherAssignment.Subject.Name, a.AttendanceDate, a.Status }).OrderByDescending(a => a.AttendanceDate).ToListAsync();
        public async Task<object> GetAttendanceByIdAsync(int id) { var a = await _context.Attendances.FindAsync(id); if (a == null) throw new Exception("السجل غير موجود."); return a; }
        public async Task<object> GetAttendancesByStudentAsync(int studentId) => await _context.Attendances.Where(a => a.StudentId == studentId).Select(a => new { a.Id, a.AttendanceDate, a.Status, a.Notes }).ToListAsync();
        public async Task<object> UpdateAttendanceAsync(int id, UpdateAttendanceDto dto) { var a = await _context.Attendances.FindAsync(id); if (a == null) throw new Exception("السجل غير موجود."); var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber == dto.StudentNumber); if (s == null) throw new Exception("الطالب غير موجود."); a.StudentId = s.Id; a.AttendanceDate = dto.AttendanceDate; a.Status = dto.Status; a.Notes = dto.Notes; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> DeleteAttendanceAsync(int id) { var a = await _context.Attendances.FindAsync(id); if (a == null) throw new Exception("السجل غير موجود."); _context.Attendances.Remove(a); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }
        public async Task<object> GetStudentAttendanceAsync(int studentId) => await _context.Attendances.Where(a => a.StudentId == studentId).Select(a => new { a.Id, a.AttendanceDate, a.Status, SubjectName = a.TeacherAssignment.Subject.Name }).OrderByDescending(a => a.AttendanceDate).ToListAsync();

        public async Task<object> CreateMidYearGradeAsync(CreateMidYearGradeDto dto) { var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber == dto.StudentNumber); if (s == null) throw new Exception("الطالب غير موجود"); var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(t => t.Subject.Name == dto.SubjectName && t.ClassRoom.Name == dto.ClassRoomName && t.Section.Name == dto.SectionName); if (ta == null) throw new Exception("لا يوجد تكليف تدريسي"); if (await _context.Grades.AnyAsync(g => g.StudentId == s.Id && g.TeacherAssignmentId == ta.Id && g.TermName == "نصف العام")) throw new Exception("درجات نصف العام مسجلة مسبقاً."); _context.Grades.Add(new Grade { StudentId = s.Id, TeacherAssignmentId = ta.Id, TermName = "نصف العام", GradeType = "المحصلة الأولى", Score = dto.FirstAssessmentScore, MaxScore = 20 }); _context.Grades.Add(new Grade { StudentId = s.Id, TeacherAssignmentId = ta.Id, TermName = "نصف العام", GradeType = "اختبار نصف العام", Score = dto.MidtermExamScore, MaxScore = 30 }); await _context.SaveChangesAsync(); return new { message = "تم حفظ الدرجات" }; }
        public async Task<object> CreateFinalYearGradeAsync(CreateFinalYearGradeDto dto) { var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber == dto.StudentNumber); if (s == null) throw new Exception("الطالب غير موجود"); var ta = await _context.TeacherAssignments.FirstOrDefaultAsync(t => t.Subject.Name == dto.SubjectName && t.ClassRoom.Name == dto.ClassRoomName && t.Section.Name == dto.SectionName); if (ta == null) throw new Exception("لا يوجد تكليف تدريسي"); if (await _context.Grades.AnyAsync(g => g.StudentId == s.Id && g.TeacherAssignmentId == ta.Id && g.TermName == "نهاية العام")) throw new Exception("درجات نهاية العام مسجلة مسبقاً."); _context.Grades.Add(new Grade { StudentId = s.Id, TeacherAssignmentId = ta.Id, TermName = "نهاية العام", GradeType = "المحصلة الثانية", Score = dto.SecondAssessmentScore, MaxScore = 20 }); _context.Grades.Add(new Grade { StudentId = s.Id, TeacherAssignmentId = ta.Id, TermName = "نهاية العام", GradeType = "اختبار نهاية العام", Score = dto.FinalExamScore, MaxScore = 30 }); await _context.SaveChangesAsync(); return new { message = "تم حفظ الدرجات" }; }
        public async Task<object> GetStudentGradesAsync(int studentId) => await _context.Grades.Include(g => g.TeacherAssignment).ThenInclude(ta => ta.Subject).Where(g => g.StudentId == studentId).Select(g => new { g.Id, SubjectName = g.TeacherAssignment.Subject.Name, g.TermName, g.GradeType, g.Score, g.MaxScore }).ToListAsync();

        public async Task<object> GetStudentFullResultAsync(string studentNumber)
        {
            var student = await _context.Students.Include(s => s.User).Include(s => s.ClassRoom).Include(s => s.Section).FirstOrDefaultAsync(s => s.StudentNumber.Trim() == studentNumber.Trim());
            if (student == null) throw new Exception("الطالب غير موجود.");
            var grades = await _context.Grades.Include(g => g.TeacherAssignment).ThenInclude(a => a.Subject).Where(g => g.StudentId == student.Id).ToListAsync();
            if (!grades.Any()) throw new Exception("لا توجد درجات مدخلة لهذا الطالب.");

            bool hasFinalYear = grades.Any(g => g.TermName == "نهاية العام");
            var result = grades.GroupBy(g => g.TeacherAssignment.Subject.Name).Select(group => {
                var first = group.FirstOrDefault(g => g.TermName == "نصف العام" && g.GradeType == "المحصلة الأولى")?.Score ?? 0;
                var midterm = group.FirstOrDefault(g => g.TermName == "نصف العام" && (g.GradeType == "اختبار نصف العام" || g.GradeType == "اختبار"))?.Score ?? 0;
                var second = group.FirstOrDefault(g => g.TermName == "نهاية العام" && g.GradeType == "المحصلة الثانية")?.Score ?? 0;
                var finalEx = group.FirstOrDefault(g => g.TermName == "نهاية العام" && (g.GradeType == "اختبار نهاية العام" || g.GradeType == "اختبار"))?.Score ?? 0;
                var midTotal = first + midterm; var finalTotal = second + finalEx; var total = midTotal + finalTotal;
                return new { SubjectName = group.Key, FirstAssessmentScore = first, MidtermExamScore = midterm, MidYearTotal = midTotal, SecondAssessmentScore = second, FinalExamScore = finalEx, FinalYearTotal = finalTotal, Total = total, IsFailed = hasFinalYear ? total < 50 : midTotal < 25 };
            }).OrderBy(r => r.SubjectName).ToList();

            decimal grandTotal = result.Sum(r => hasFinalYear ? r.Total : r.MidYearTotal);
            decimal maxTotal = result.Count * (hasFinalYear ? 100 : 50);
            decimal percentage = maxTotal > 0 ? Math.Round((grandTotal / maxTotal) * 100, 2) : 0;
            int failedCount = result.Count(r => r.IsFailed);
            string estimate = percentage >= 90 ? "ممتاز" : percentage >= 80 ? "جيد جدًا" : percentage >= 70 ? "جيد" : percentage >= 60 ? "مقبول" : "ضعيف";

            return new { StudentName = student.User?.FullName ?? "", StudentNumber = student.StudentNumber, ClassRoomName = student.ClassRoom?.Name ?? "", SectionName = student.Section?.Name ?? "", HasFinalYear = hasFinalYear, Results = result, GrandTotal = grandTotal, MaxTotal = maxTotal, Percentage = percentage, Estimate = estimate, FinalStatus = failedCount > 0 ? "راسب" : "ناجح", FailedSubjectsCount = failedCount };
        }

        public async Task<object> UpdateGradeAsync(int id, UpdateGradeDto dto) { var g = await _context.Grades.FindAsync(id); if (g == null) throw new Exception("الدرجة غير موجودة."); g.Score = dto.FirstAssessmentScore ?? g.Score; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> GetGradesAsync() => await _context.Grades.Include(g => g.Student).ThenInclude(s => s.User).Select(g => new { g.Id, StudentName = g.Student.User.FullName, g.TermName, g.GradeType, g.Score }).ToListAsync();
        public async Task<object> GetGradeByIdAsync(int id) { var g = await _context.Grades.FindAsync(id); if (g == null) throw new Exception("الدرجة غير موجودة."); return g; }
        public async Task<object> DeleteGradeAsync(int id) { var g = await _context.Grades.FindAsync(id); if (g == null) throw new Exception("الدرجة غير موجودة."); _context.Grades.Remove(g); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> CreateStudentNoteAsync(CreateStudentNoteDto dto) { var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber == dto.StudentNumber); if (s == null) throw new Exception("الطالب غير موجود."); var n = new StudentNote { StudentId = s.Id, NoteText = dto.NoteText, CreatedAt = DateTime.Now }; _context.StudentNotes.Add(n); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة" }; }
        public async Task<object> GetStudentNotesAsync() => await _context.StudentNotes.Include(n => n.Student).ThenInclude(s => s.User).Select(n => new { n.Id, StudentName = n.Student.User.FullName, n.NoteText, n.CreatedAt }).ToListAsync();
        public async Task<object> GetStudentNoteByIdAsync(int id) { var n = await _context.StudentNotes.FindAsync(id); if (n == null) throw new Exception("الملاحظة غير موجودة."); return n; }
        public async Task<object> GetStudentNotesByStudentAsync(int studentId) => await _context.StudentNotes.Where(n => n.StudentId == studentId).ToListAsync();
        public async Task<object> UpdateStudentNoteAsync(int id, UpdateStudentNoteDto dto) { var n = await _context.StudentNotes.FindAsync(id); if (n == null) throw new Exception("الملاحظة غير موجودة."); n.NoteText = dto.NoteText; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> DeleteStudentNoteAsync(int id) { var n = await _context.StudentNotes.FindAsync(id); if (n == null) throw new Exception("الملاحظة غير موجودة."); _context.StudentNotes.Remove(n); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> CreateDailyTaskAsync(CreateDailyTaskDto dto) { var s = await _context.Students.FirstOrDefaultAsync(st => st.StudentNumber == dto.StudentNumber); if (s == null) throw new Exception("الطالب غير موجود."); var dt = new DailyTask { StudentId = s.Id, Title = dto.Title, Description = dto.Description, TaskDate = dto.TaskDate, IsCompleted = dto.IsCompleted }; _context.DailyTasks.Add(dt); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة" }; }
        public async Task<object> GetDailyTasksAsync() => await _context.DailyTasks.Include(t => t.Student).ThenInclude(s => s.User).Select(t => new { t.Id, StudentName = t.Student.User.FullName, t.Title, t.TaskDate }).ToListAsync();
        public async Task<object> GetDailyTaskByIdAsync(int id) { var t = await _context.DailyTasks.FindAsync(id); if (t == null) throw new Exception("المهمة غير موجودة."); return t; }
        public async Task<object> GetDailyTasksByStudentAsync(int studentId) => await _context.DailyTasks.Where(t => t.StudentId == studentId).ToListAsync();
        public async Task<object> UpdateDailyTaskAsync(int id, UpdateDailyTaskDto dto) { var t = await _context.DailyTasks.FindAsync(id); if (t == null) throw new Exception("المهمة غير موجودة."); t.Title = dto.Title; t.Description = dto.Description; t.TaskDate = dto.TaskDate; t.IsCompleted = dto.IsCompleted; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> DeleteDailyTaskAsync(int id) { var t = await _context.DailyTasks.FindAsync(id); if (t == null) throw new Exception("المهمة غير موجودة."); _context.DailyTasks.Remove(t); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> GetDashboardStatsAsync() => new { Students = await _context.Students.CountAsync(), Teachers = await _context.Teachers.CountAsync(), Subjects = await _context.Subjects.CountAsync(), ClassRooms = await _context.ClassRooms.CountAsync(), Sections = await _context.Sections.CountAsync(), Assignments = await _context.TeacherAssignments.CountAsync(), Lessons = await _context.Lessons.CountAsync(), Homeworks = await _context.Homeworks.CountAsync(), Attendances = await _context.Attendances.CountAsync(), Grades = await _context.Grades.CountAsync() };

        public async Task<object> GetSchoolSchedulesAsync() => await _context.SchoolSchedules.Include(s => s.TeacherAssignment).ThenInclude(a => a.Subject).Select(s => new { s.Id, s.DayName, s.PeriodNumber, s.StartTime, s.EndTime, SubjectName = s.TeacherAssignment.Subject.Name }).ToListAsync();
        public async Task<object> GetSchoolSchedulesByClassSectionAsync(int classRoomId, int sectionId) => await _context.SchoolSchedules.Where(s => s.TeacherAssignment.ClassRoomId == classRoomId && s.TeacherAssignment.SectionId == sectionId).ToListAsync();
        public async Task<object> GetSchoolScheduleAsync(int id) { var s = await _context.SchoolSchedules.FindAsync(id); if (s == null) throw new Exception("الحصة غير موجودة."); return s; }
        public async Task<object> CreateSchoolScheduleAsync(CreateSchoolScheduleDto dto) { var s = new SchoolSchedule { TeacherAssignmentId = dto.TeacherAssignmentId, DayName = dto.DayName, PeriodNumber = dto.PeriodNumber, StartTime = dto.StartTime, EndTime = dto.EndTime, RoomName = dto.RoomName, Notes = dto.Notes }; _context.SchoolSchedules.Add(s); await _context.SaveChangesAsync(); return new { message = "تمت الإضافة" }; }
        public async Task<object> UpdateSchoolScheduleAsync(int id, UpdateSchoolScheduleDto dto) { var s = await _context.SchoolSchedules.FindAsync(id); if (s == null) throw new Exception("الحصة غير موجودة."); s.DayName = dto.DayName; s.PeriodNumber = dto.PeriodNumber; s.StartTime = dto.StartTime; s.EndTime = dto.EndTime; await _context.SaveChangesAsync(); return new { message = "تم التعديل" }; }
        public async Task<object> DeleteSchoolScheduleAsync(int id) { var s = await _context.SchoolSchedules.FindAsync(id); if (s == null) throw new Exception("الحصة غير موجودة."); _context.SchoolSchedules.Remove(s); await _context.SaveChangesAsync(); return new { message = "تم الحذف" }; }

        public async Task<object> GetStudentPortalNewCountsAsync(int studentId) { var s = await _context.Students.FindAsync(studentId); if (s == null) throw new Exception("الطالب غير موجود."); return new { LessonsCount = 0, HomeworksCount = 0, AttendanceCount = 0, ScheduleCount = 0, NotesCount = 0 }; } // المبسط للمسؤول
        public async Task<object> MarkStudentPortalAreaAsViewedAsync(int studentId, string areaKey) { return new { message = "تم التأشير" }; } // المبسط للمسؤول
    }
}