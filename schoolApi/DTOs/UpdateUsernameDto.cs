using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستقبل اسم المستخدم الجديد
        عند تعديل اسم المستخدم من مدير النظام الرئيسي.
    */
    public class UpdateUsernameDto
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;
    }
}