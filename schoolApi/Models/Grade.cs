namespace schoolApi.Models
{
    /*
       هذا الكلاس يمثل جدول Grades

       وظيفة هذا الجدول:
       تخزين نتائج الطالب في المواد

       كل درجة مرتبطة بـ:
       - طالب
       - مادة/مدرس/صف/شعبة عبر TeacherAssignment
   */
    public class Grade
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // التكليف التدريسي
        public int TeacherAssignmentId { get; set; }
        public TeacherAssignment? TeacherAssignment { get; set; }

        // نوع الاختبار
        public string Part { get; set; } = string.Empty;
        public string GradeType { get; set; } = string.Empty;

        // الدرجة التي حصل عليها الطالب
        public decimal Score { get; set; }

        // الدرجة الكلية
        public decimal MaxScore { get; set; }
        public string TermName { get; set; } = string.Empty;

        // ملاحظات
        public string? Notes { get; set; }
    }
}
