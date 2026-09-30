namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يمثل لوحة الطالب الرئيسية

        يعرض:
        - بيانات الطالب الأساسية
        - الصف والشعبة
        - روابط سريعة لباقي صفحات الطالب
    */
    public class StudentDashboardViewModel
    {
        public int Id { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Username { get; set; }

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ParentName { get; set; }

        public string? ParentPhone { get; set; }

        public int? ClassRoomId { get; set; }

        public string? ClassRoomName { get; set; }

        public int? SectionId { get; set; }

        public string? SectionName { get; set; }

        /*
    عدادات لوحة الطالب

    هذه الأرقام تظهر في لوحة الطالب
    حتى يعرف الطالب عدد الأشياء المرتبطة به.
*/
        public int LessonsCount { get; set; }

        public int HomeworksCount { get; set; }

        public int NotesCount { get; set; }

        public int AttendanceCount { get; set; }

        public int ScheduleCount { get; set; }
    }
}