using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدمه المدرس لإضافة حضور أو غياب

        المدرس لا يرسل رقم المدرس
        لأن النظام يعرفه من التوكن
    */
    public class TeacherCreateAttendanceDto
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