namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لاستقبال بيانات إنشاء مدرس جديد

        عند إنشاء مدرس جديد فنحن ننشئ:
        1- حساب دخول في جدول Users
        2- بيانات تفصيلية في جدول Teachers
    */
    public class CreateTeacherDto
    {
        // بيانات الدخول
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // البيانات التفصيلية للمدرس
        //public string TeacherNumber { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }
    }
}
