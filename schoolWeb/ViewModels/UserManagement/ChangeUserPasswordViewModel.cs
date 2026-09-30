using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.UserManagement
{
    /*
        هذا الكلاس يستخدم عندما يقوم المدير
        بتغيير كلمة مرور مستخدم معين

        مثال:
        - طالب نسي كلمة المرور
        - مدرس نسي كلمة المرور
        - المدير يريد إعادة تعيينها
    */
    public class ChangeUserPasswordViewModel
    {
        // رقم المستخدم الذي سيتم تغيير كلمة مروره
        public int UserId { get; set; }

        // الاسم للعرض فقط
        public string FullName { get; set; } = string.Empty;

        // اسم المستخدم للعرض فقط
        public string Username { get; set; } = string.Empty;

        // كلمة المرور الجديدة
        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        public string NewPassword { get; set; } = string.Empty;

        // تأكيد كلمة المرور الجديدة
        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}