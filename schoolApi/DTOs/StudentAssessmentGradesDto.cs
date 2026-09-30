namespace schoolApi.DTOs
{
    /*
        هذا DTO يمثل نتيجة المحصلة كاملة.

        يحتوي:
        - اسم الصف
        - اسم الشعبة
        - اسم المحصلة
        - قائمة درجات المواد
    */
    public class StudentAssessmentGradesDto
    {
        public int ClassRoomId { get; set; }

        public string ClassRoomName { get; set; } = string.Empty;

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;

        public string AssessmentType { get; set; } = string.Empty;

        public string AssessmentTitle { get; set; } = string.Empty;

        public List<StudentAssessmentGradeItemDto> Items { get; set; } = new();
    }
}