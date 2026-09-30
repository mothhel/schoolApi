using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
       هذا الكلاس يستخدم لتعديل بيانات الشعبة
       ويمكن تعديل:
       - اسم الشعبة
       - الصف الذي تتبعه الشعبة
   */
    public class UpdateSectionDto
    {
        [Required(ErrorMessage = "اسم الشعبة الجديد مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;
    }
}
