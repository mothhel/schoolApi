using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Attendance
{
    /*
        هذا الكلاس يستخدم عند إضافة سجل حضور أو غياب جديد
    */
    public class CreateAttendanceViewModel
    {
        [Required(ErrorMessage = "الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;
        // 🔥 هذا هو المهم
        [Required(ErrorMessage = "المادة مطلوبة")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "تاريخ الحضور مطلوب")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "حالة الحضور مطلوبة")]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public List<SelectListItem> Students { get; set; } = new();

        public List<SelectListItem> TeacherAssignments { get; set; } = new();
    }
}