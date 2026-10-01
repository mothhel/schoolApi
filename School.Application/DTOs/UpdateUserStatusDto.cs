using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل حالة حساب المستخدم

        IsActive = true  يعني الحساب مفعل
        IsActive = false يعني الحساب معطل
    */
    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}