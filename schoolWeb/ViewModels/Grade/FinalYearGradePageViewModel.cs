using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يمثل صفحة إدخال درجات نهاية العام

        طريقة العمل:
        - اختيار الصف
        - اختيار الشعبة
        - اختيار المادة
        - عرض طلاب الصف والشعبة
        - إدخال درجات نهاية العام لمادة واحدة لكل طالب
    */
    public class FinalYearGradePageViewModel
    {
        [Required(ErrorMessage = "الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        public string SectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "المادة مطلوبة")]
        public string SubjectName { get; set; } = string.Empty;

        /*
            قائمة الطلاب الذين سيتم عرضهم في الجدول
        */
        public List<FinalYearStudentGradeRowViewModel> Students { get; set; } = new();

        /*
            القوائم المنسدلة
        */
        public List<SelectListItem> ClassRooms { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();

        public List<SelectListItem> Subjects { get; set; } = new();
    }
}