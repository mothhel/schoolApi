using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يمثل صفحة إدخال درجات نصف العام

        طريقة العمل:
        - اختيار الصف
        - اختيار الشعبة
        - اختيار المادة
        - عرض طلاب الصف والشعبة
        - إدخال درجات مادة واحدة لكل طالب
    */
    public class MidYearGradePageViewModel
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
        public List<MidYearStudentGradeRowViewModel> Students { get; set; } = new();

        /*
            القوائم المنسدلة
        */
        public List<SelectListItem> ClassRooms { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();

        public List<SelectListItem> Subjects { get; set; } = new();
    }
}