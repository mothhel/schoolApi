namespace School.Application.DTOs
{
    /*
        هذا DTO يمثل جزء واحد من نتيجة المادة

        مثال:
        - المحصلة الأولى
        - نتيجة نصف العام
        - المحصلة الثانية
        - نتيجة نهاية العام
    */
    public class StudentResultComponentDto
    {
        public string Key { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal MaxScore { get; set; }
    }
}