using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة درجة جديدة لطالب
    */
    public class CreateGradeDto
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

        [Required(ErrorMessage = "الجزء الدراسي مطلوب")]
        public string Part { get; set; } = string.Empty; // الأول أو الثاني

        [Required(ErrorMessage = "نوع الدرجة مطلوب")]
        public string GradeType { get; set; } = string.Empty; // شهري - سلوك - مواظبة - نهائي

        [Required(ErrorMessage = "الدرجة مطلوبة")]
        public decimal Score { get; set; }

        [Required(ErrorMessage = "الدرجة الكلية مطلوبة")]
        public decimal MaxScore { get; set; }

        public string? Notes { get; set; }
    }
}