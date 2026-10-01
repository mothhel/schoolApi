using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستقبل بيانات تعديل حصة في الجدول الدراسي
    */
    public class UpdateSchoolScheduleDto
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "اليوم مطلوب")]
        public string DayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الحصة مطلوب")]
        public int PeriodNumber { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        public string? RoomName { get; set; }

        public string? Notes { get; set; }
    }
}