using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدمه المدرس لتعديل درجة طالب
    */
    public class TeacherUpdateGradeDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "الجزء الدراسي مطلوب")]
        public string Part { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع الدرجة مطلوب")]
        public string GradeType { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدرجة مطلوبة")]
        public decimal Score { get; set; }

        [Required(ErrorMessage = "الدرجة الكلية مطلوبة")]
        public decimal MaxScore { get; set; }

        public string? Notes { get; set; }
    }
}