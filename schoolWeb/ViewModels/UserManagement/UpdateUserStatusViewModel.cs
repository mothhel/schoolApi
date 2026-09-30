using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.UserManagement
{
    /*
        هذا الكلاس يستخدم عند تعديل حالة حساب المستخدم

        مثل:
        - تفعيل الحساب
        - تعطيل الحساب

        مثال:
        إذا أردنا منع طالب أو مدرس من تسجيل الدخول مؤقتًا
        نجعل IsActive = false
    */
    public class UpdateUserStatusViewModel
    {
        // رقم المستخدم
        public int Id { get; set; }

        // الاسم للعرض فقط
        public string FullName { get; set; } = string.Empty;

        // اسم المستخدم للعرض فقط
        public string Username { get; set; } = string.Empty;

        // حالة الحساب
        [Required(ErrorMessage = "حالة الحساب مطلوبة")]
        public bool IsActive { get; set; }
    }
}