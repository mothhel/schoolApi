using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لاستقبال بيانات إضافة شعبة جديدة

        الإدخال هنا سيكون:
        - اسم الشعبة
        - اسم الصف الذي تتبعه

        مثال:
        Name = A
        ClassRoomName = الصف الأول
    */
    public class CreateSectionDto
    {
        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;
    }
}