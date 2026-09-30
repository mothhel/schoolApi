using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا DTO يستقبل بيانات إضافة درس جديد من تطبيق الأستاذ.
    */
    public class TeacherCreateLessonDto
    {
        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ الدرس مطلوب")]
        public DateTime LessonDate { get; set; }
    }
}