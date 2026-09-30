namespace schoolApi.DTOs
{
    /*
        هذا DTO يمثل درجة مادة واحدة داخل المحصلة.

        مثال:
        المادة: رياضيات
        المدرس: أ. أحمد
        الدرجة: 18 من 20
    */
    public class StudentAssessmentGradeItemDto
    {
        public string SubjectName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal MaxScore { get; set; } = 20;

        public string? Notes { get; set; }
    }
}