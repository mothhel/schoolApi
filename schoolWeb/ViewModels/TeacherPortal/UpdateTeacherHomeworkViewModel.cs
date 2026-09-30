using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يستخدم عند تعديل واجب موجود

        يحتوي على:
        - Id الواجب
        - التكليف التدريسي
        - عنوان الواجب
        - الوصف
        - تاريخ التسليم
    */
    public class UpdateTeacherHomeworkViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الواجب مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ التسليم مطلوب")]
        public DateTime DueDate { get; set; }

        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}