using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة درس جديد
    */
    public class CreateLessonDto
    {
        [Required(ErrorMessage = "رقم التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ الدرس مطلوب")]
        public DateTime LessonDate { get; set; }
    }
}