using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Attendance
{
    /*
        هذا الكلاس يستخدم عند تعديل سجل حضور أو غياب
    */
    public class UpdateAttendanceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;
        // 🔥 هذا هو المهم
        [Required(ErrorMessage = "المادة مطلوبة")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "تاريخ الحضور مطلوب")]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "حالة الحضور مطلوبة")]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public List<SelectListItem> Students { get; set; } = new();

        public List<SelectListItem> TeacherAssignments { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
    }
}