using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Lesson
{
    /*
        هذا الكلاس يستخدم عند إضافة درس جديد

        المدير يختار:
        - التكليف التدريسي
        ثم يكتب:
        - عنوان الدرس
        - وصف الدرس
        - تاريخ الدرس
    */
    public class CreateLessonViewModel
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ الدرس مطلوب")]
        public DateTime LessonDate { get; set; } = DateTime.Today;

        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}