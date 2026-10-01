

namespace School.Application.DTOs
{
    /*
        يمثل نتيجة مادة واحدة للطالب.

        يحتوي على:
        - اسم المادة
        - اسم المدرس
        - تفاصيل الدرجات
        - المجموع
        - النسبة
        - الحالة
    */
    public class StudentResultSubjectDto
    {
        public string SubjectName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public List<StudentResultComponentDto> Components { get; set; } = new();

        public decimal TotalScore { get; set; }

        public decimal TotalMaxScore { get; set; }

        public decimal Percentage { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}