using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Teacher
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات المدرس من واجهة المدير

        يحتوي على:
        - بيانات الحساب
        - بيانات المدرس
        - كلمة مرور اختيارية
        - حالة الحساب
    */
    public class UpdateTeacherViewModel
    {
        public int Id { get; set; }

        //[Required(ErrorMessage = "رقم المدرس مطلوب")]
        public string TeacherNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المدرس مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        /*
            كلمة المرور اختيارية
            إذا تركها المدير فارغة لا تتغير كلمة المرور
        */
        public string? Password { get; set; }

        public string? Specialty { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? HireDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}