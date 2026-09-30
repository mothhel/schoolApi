using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.UserManagement
{
    /*
        هذا الكلاس يستخدم لتعديل اسم المستخدم
        من صفحة إدارة المستخدمين.
    */
    public class EditUsernameViewModel
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string CurrentUsername { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم الجديد مطلوب")]
        public string NewUsername { get; set; } = string.Empty;
    }
}