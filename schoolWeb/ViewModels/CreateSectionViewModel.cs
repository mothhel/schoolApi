using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم لإضافة شعبة جديدة من الواجهة

        تم تحسينه ليحتوي على:
        - اسم الشعبة
        - اسم الصف
        - قائمة الصفوف لعرضها داخل Dropdown
    */
    public class CreateSectionViewModel
    {
        // اسم الشعبة مطلوب
        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string Name { get; set; } = string.Empty;

        // اسم الصف مطلوب
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        /*
            هذه القائمة سنملؤها من الكنترولر
            حتى نعرض الصفوف داخل قائمة منسدلة
        */
        public List<SelectListItem> ClassRooms { get; set; } = new();
    }
}