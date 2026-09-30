using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لتغيير كلمة المرور

        المستخدم يجب أن يرسل:
        - كلمة المرور الحالية
        - كلمة المرور الجديدة
        - تأكيد كلمة المرور الجديدة
    */
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور الجديدة مطلوب")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}