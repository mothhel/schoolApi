using Microsoft.EntityFrameworkCore;
using school.Domain.Entities; // استخدام الـ namespace الجديد

namespace school.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Role> Roles { get; }
        DbSet<User> Users { get; }
        DbSet<ClassRoom> ClassRooms { get; }
        DbSet<Section> Sections { get; }
        DbSet<Subject> Subjects { get; }
        DbSet<Student> Students { get; }
        DbSet<Teacher> Teachers { get; }
        DbSet<TeacherAssignment> TeacherAssignments { get; }
        DbSet<StudentEnrollment> StudentEnrollments { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<Homework> Homeworks { get; }
        DbSet<Attendance> Attendances { get; }
        DbSet<Grade> Grades { get; }
        DbSet<StudentNote> StudentNotes { get; }
        DbSet<DailyTask> DailyTasks { get; }
        DbSet<SchoolSchedule> SchoolSchedules { get; }
        DbSet<StudentPortalReadStatus> StudentPortalReadStatuses { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}