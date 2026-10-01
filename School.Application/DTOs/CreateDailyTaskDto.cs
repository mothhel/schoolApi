using System.ComponentModel.DataAnnotations;

namespace School.Application.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة مهمة يومية جديدة لطالب
    */
    public class CreateDailyTaskDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان المهمة مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "تاريخ المهمة مطلوب")]
        public DateTime TaskDate { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}