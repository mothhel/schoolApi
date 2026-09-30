namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستخدم لعرض الدروس الخاصة بالطالب

        الطالب يرى فقط الدروس المرتبطة بصفه وشعبته
    */
    public class StudentLessonViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime LessonDate { get; set; }

        public string? TeacherName { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }
    }
}