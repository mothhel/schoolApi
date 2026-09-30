namespace schoolWeb.ViewModels.Homework
{
    /*
        هذا الكلاس يستخدم لعرض الواجبات في صفحة إدارة الواجبات
    */
    public class HomeworkViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string? TeacherName { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }
    }
}