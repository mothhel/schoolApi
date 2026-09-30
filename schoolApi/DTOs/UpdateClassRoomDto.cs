using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    
        /*
       هذا الكلاس يستخدم لتعديل اسم الصف
   */
        public class UpdateClassRoomDto
        {
            [Required(ErrorMessage = "اسم الصف الجديد مطلوب")]
            public string Name { get; set; } = string.Empty;
        }
}
