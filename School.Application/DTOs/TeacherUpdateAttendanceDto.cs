using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدمه المدرس لتعديل سجل حضور أو غياب
    */
    public class TeacherUpdateAttendanceDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "تاريخ الحضور مطلوب")]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "حالة الحضور مطلوبة")]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}