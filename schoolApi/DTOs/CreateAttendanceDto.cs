using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة سجل حضور أو غياب جديد
    */
    public class CreateAttendanceDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم المدرس مطلوب")]
        public string TeacherNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المادة مطلوب")]
        public string SubjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string SectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الحضور مطلوب")]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "حالة الحضور مطلوبة")]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}