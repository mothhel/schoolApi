using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Homework
{
    /*
        هذا الكلاس يستخدم عند إضافة واجب جديد
    */
    public class CreateHomeworkViewModel
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الواجب مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ التسليم مطلوب")]
        public DateTime DueDate { get; set; } = DateTime.Today;

        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}