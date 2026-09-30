using Microsoft.EntityFrameworkCore;
using schoolApi.Models;

namespace schoolApi.Data
{
    /*
        هذا الكلاس هو المسؤول عن الربط بين الكلاسات وقاعدة البيانات

        كل DbSet يمثل جدولاً في قاعدة البيانات

        ومن خلال OnModelCreating نقوم بتحديد:
        - العلاقات بين الجداول
        - المفاتيح الفريدة
        - القيود الخاصة
    */
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<StudentNote> StudentNotes { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<SchoolSchedule> SchoolSchedules { get; set; }
        public DbSet<StudentPortalReadStatus> StudentPortalReadStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // جعل اسم الدور فريداً
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // جعل اسم المستخدم فريداً
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // جعل رقم الطالب فريداً
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            // جعل رقم المدرس فريداً
            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.TeacherNumber)
                .IsUnique();

            // جعل اسم المادة فريداً
            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.Name)
                .IsUnique();

            /*
                Seed Data
                هذا الجزء يقوم بإضافة الأدوار الأساسية تلقائياً عند إنشاء قاعدة البيانات أو تحديثها
                بحيث لا نحتاج لإدخالها يدويًا كل مرة
            */
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Teacher" },
                new Role { Id = 3, Name = "Student" }
            );

            /*
                علاقة One-to-One بين User و Student
                المستخدم الواحد الطالب له سجل طالب واحد فقط
            */
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة One-to-One بين User و Teacher
                المستخدم الواحد المدرس له سجل مدرس واحد فقط
            */
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithOne(u => u.Teacher)
                .HasForeignKey<Teacher>(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة الصف مع الشعب
                الصف الواحد له عدة شعب
            */
            modelBuilder.Entity<Section>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة الصف مع الطلاب
            */
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة الشعبة مع الطلاب
            */
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Section)
                .WithMany(sec => sec.Students)
                .HasForeignKey(s => s.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة TeacherAssignment مع Teacher
            */
            modelBuilder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.Teacher)
                .WithMany(t => t.TeacherAssignments)
                .HasForeignKey(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة TeacherAssignment مع Subject
            */
            modelBuilder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.Subject)
                .WithMany(s => s.TeacherAssignments)
                .HasForeignKey(ta => ta.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة TeacherAssignment مع ClassRoom
            */
            modelBuilder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.ClassRoom)
                .WithMany(c => c.TeacherAssignments)
                .HasForeignKey(ta => ta.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                علاقة TeacherAssignment مع Section
            */
            modelBuilder.Entity<TeacherAssignment>()
                .HasOne(ta => ta.Section)
                .WithMany(s => s.TeacherAssignments)
                .HasForeignKey(ta => ta.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الطالب مع المواد المسجل بها
            */
            modelBuilder.Entity<StudentEnrollment>()
                .HasOne(se => se.Student)
                .WithMany(s => s.StudentEnrollments)
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentEnrollment>()
                .HasOne(se => se.TeacherAssignment)
                .WithMany(ta => ta.StudentEnrollments)
                .HasForeignKey(se => se.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الدروس بالتكليف التدريسي
            */
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.TeacherAssignment)
                .WithMany(ta => ta.Lessons)
                .HasForeignKey(l => l.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
                ربط الواجبات بالتكليف التدريسي
            */
            modelBuilder.Entity<Homework>()
                .HasOne(h => h.TeacherAssignment)
                .WithMany(ta => ta.Homeworks)
                .HasForeignKey(h => h.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
                ربط الحضور بالطالب
            */
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الحضور بالتكليف التدريسي
            */
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.TeacherAssignment)
                .WithMany(ta => ta.Attendances)
                .HasForeignKey(a => a.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الدرجات بالطالب
            */
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الدرجات بالتكليف التدريسي
            */
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.TeacherAssignment)
                .WithMany(ta => ta.Grades)
                .HasForeignKey(g => g.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
                ربط الملاحظات بالطالب
            */
            modelBuilder.Entity<StudentNote>()
                .HasOne(sn => sn.Student)
                .WithMany(s => s.StudentNotes)
                .HasForeignKey(sn => sn.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
                ربط المهام اليومية بالطالب
            */
            modelBuilder.Entity<DailyTask>()
                .HasOne(dt => dt.Student)
                .WithMany(s => s.DailyTasks)
                .HasForeignKey(dt => dt.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
