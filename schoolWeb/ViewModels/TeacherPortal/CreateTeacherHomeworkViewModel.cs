using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يستخدم عند إضافة واجب جديد من واجهة المدرس

        المدرس يختار:
        - المادة / الصف / الشعبة من تكليفاته فقط

        ثم يدخل:
        - عنوان الواجب
        - وصف الواجب
        - تاريخ التسليم
    */
    public class CreateTeacherHomeworkViewModel
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الواجب مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ التسليم مطلوب")]
        public DateTime DueDate { get; set; } = DateTime.Today;

        /*
            قائمة التكليفات الخاصة بالمدرس
            تظهر في Dropdown
        */
        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}