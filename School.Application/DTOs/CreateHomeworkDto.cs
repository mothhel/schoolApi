using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة واجب جديد
        الواجب مرتبط بتكليف تدريسي
    */
    public class CreateHomeworkDto
    {
        [Required(ErrorMessage = "رقم التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الواجب مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // تاريخ التسليم
        public DateTime? DueDate { get; set; }
    }
}