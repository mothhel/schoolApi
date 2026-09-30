using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.TeacherAssignment
{
    /*
        هذا الكلاس يستخدم عند إضافة تكليف تدريسي جديد

        المدير سيختار:
        - المدرس
        - المادة
        - الصف
        - الشعبة
    */
    public class CreateTeacherAssignmentViewModel
    {
        [Required(ErrorMessage = "المدرس مطلوب")]
        public string TeacherNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "المادة مطلوبة")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        public string SectionName { get; set; } = string.Empty;

        /*
            القوائم المنسدلة
        */
        public List<SelectListItem> Teachers { get; set; } = new();

        public List<SelectListItem> Subjects { get; set; } = new();

        public List<SelectListItem> ClassRooms { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();
    }
}