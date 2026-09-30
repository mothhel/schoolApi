using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يستخدم عند إضافة درس جديد من واجهة المدرس

        المدرس يختار التكليف التدريسي الخاص به
        ثم يدخل:
        - عنوان الدرس
        - وصف الدرس
        - تاريخ الدرس
    */
    public class CreateTeacherLessonViewModel
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ الدرس مطلوب")]
        public DateTime LessonDate { get; set; } = DateTime.Today;

        /*
            قائمة التكليفات الخاصة بالمدرس
            تظهر في Dropdown
        */
        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}