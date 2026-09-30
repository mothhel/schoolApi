using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات الشعبة

        يحتوي على:
        - رقم الشعبة
        - اسم الشعبة
        - اسم الصف
        - قائمة الصفوف للعرض في Dropdown
    */
    public class UpdateSectionViewModel
    {
        // رقم الشعبة
        public int Id { get; set; }

        // اسم الشعبة
        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string Name { get; set; } = string.Empty;

        // اسم الصف المرتبط بها
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        // قائمة الصفوف
        public List<SelectListItem> ClassRooms { get; set; } = new();
    }
}