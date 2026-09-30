namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستخدم لعرض درجات الطالب

        نحتاج:
        - المادة
        - المدرس
        - الصف
        - الشعبة
        - نوع المحصلة
        - الدرجة
    */
    public class StudentGradeViewModel
    {
        public int Id { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string TermName { get; set; } = string.Empty;

        public string GradeType { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal MaxScore { get; set; }
    }
}