using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لاستقبال بيانات تسجيل الدخول

        المستخدم يدخل:
        - اسم المستخدم
        - كلمة المرور
    */
    public class LoginDto
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;
    }
}