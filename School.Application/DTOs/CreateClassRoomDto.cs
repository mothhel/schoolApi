using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لاستقبال بيانات إضافة صف جديد

        مثال:
        الصف الأول
        الصف الثاني
        الصف الثالث
    */
    public class CreateClassRoomDto
    {
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}
