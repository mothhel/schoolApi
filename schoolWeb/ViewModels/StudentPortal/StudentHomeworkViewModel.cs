namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستخدم لعرض واجبات الطالب

        الطالب يرى فقط الواجبات الخاصة بصفه وشعبته

        مهم:
        ClassRoomName و SectionName نحتاجها حتى نفلتر الواجبات
        حسب صف وشعبة الطالب.
    */
    public class StudentHomeworkViewModel
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        public string? TeacherName { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }
    }
}