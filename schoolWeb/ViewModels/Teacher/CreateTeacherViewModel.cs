using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Teacher
{
    /*
        هذا الكلاس يستخدم عند إضافة مدرس جديد
    */
    public class CreateTeacherViewModel
    {
        [Required(ErrorMessage = "اسم المدرس مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;

        //[Required(ErrorMessage = "رقم المدرس مطلوب")]
        //public string TeacherNumber { get; set; } = string.Empty;

        public string? Specialty { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? HireDate { get; set; }
    }
}