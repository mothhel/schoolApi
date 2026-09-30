namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يستخدم لعرض دروس المدرس في صفحة الدروس

        كل درس مرتبط بتكليف تدريسي
        والتكليف التدريسي يحتوي على:
        - المادة
        - الصف
        - الشعبة
    */
    public class TeacherLessonViewModel
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime LessonDate { get; set; }

        public string? TeacherName { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }
    }
}