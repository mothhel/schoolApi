using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لاستقبال بيانات إنشاء طالب جديد

        عند إنشاء طالب جديد فنحن ننشئ:
        1- حساب دخول في جدول Users
        2- بيانات تفصيلية في جدول Students
    */
    public class CreateStudentDto
    {
        // بيانات الدخول
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // بيانات الطالب التفصيلية
        // public string StudentNumber { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }

        // الصف والشعبة
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string SectionName { get; set; } = string.Empty;
    }
}
