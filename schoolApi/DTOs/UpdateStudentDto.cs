using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات الطالب
        لاحظ أننا لا نعدل كلمة المرور هنا
    */
    public class UpdateStudentDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }

        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }

        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string SectionName { get; set; } = string.Empty;

        // هل حساب الطالب مفعل
        public bool IsActive { get; set; } = true;
    }
}