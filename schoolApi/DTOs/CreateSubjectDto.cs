using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة مادة جديدة
    */
    public class CreateSubjectDto
    {
        [Required(ErrorMessage = "اسم المادة مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}