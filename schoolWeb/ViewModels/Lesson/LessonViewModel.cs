namespace schoolWeb.ViewModels.Lesson
{
    /*
        هذا الكلاس يستخدم لعرض الدروس في صفحة الإدارة

        يحتوي على:
        - بيانات الدرس
        - بيانات المدرس
        - المادة
        - الصف
        - الشعبة
    */
    public class LessonViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime LessonDate { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string? TeacherName { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }
    }
}