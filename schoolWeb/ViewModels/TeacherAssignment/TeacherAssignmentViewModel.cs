namespace schoolWeb.ViewModels.TeacherAssignment
{
    /*
        هذا الكلاس يستخدم لعرض التكليفات التدريسية في الواجهة

        التكليف التدريسي يعني:
        - مدرس
        - مادة
        - صف
        - شعبة
    */
    public class TeacherAssignmentViewModel
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public string TeacherNumber { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int ClassRoomId { get; set; }

        public string ClassRoomName { get; set; } = string.Empty;

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;
    }
}