using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات المدرس
        لاحظ أننا لا نعدل كلمة المرور هنا
    */
    public class UpdateTeacherDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }

        [Required(ErrorMessage = "رقم المدرس مطلوب")]
        public string TeacherNumber { get; set; } = string.Empty;

        public string? Specialty { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }

        // هل حساب المدرس مفعل
        public bool IsActive { get; set; } = true;
    }
}