using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل اسم المادة
    */
    public class UpdateSubjectDto
    {
        [Required(ErrorMessage = "اسم المادة الجديد مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}