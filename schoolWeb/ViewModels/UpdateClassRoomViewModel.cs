using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات الصف
    */
    public class UpdateClassRoomViewModel
    {
        // رقم الصف
        public int Id { get; set; }

        // اسم الصف الجديد
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}