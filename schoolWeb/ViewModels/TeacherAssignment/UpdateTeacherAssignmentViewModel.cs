using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.TeacherAssignment
{
    /*
        هذا الكلاس يستخدم عند تعديل تكليف تدريسي

        يحتوي على نفس بيانات الإضافة
        بالإضافة إلى Id التكليف
    */
    public class UpdateTeacherAssignmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "المدرس مطلوب")]
        public string TeacherNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "المادة مطلوبة")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        public string SectionName { get; set; } = string.Empty;

        public List<SelectListItem> Teachers { get; set; } = new();

        public List<SelectListItem> Subjects { get; set; } = new();

        public List<SelectListItem> ClassRooms { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();
    }
}