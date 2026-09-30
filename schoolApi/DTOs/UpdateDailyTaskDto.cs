using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل مهمة يومية للطالب
    */
    public class UpdateDailyTaskDto
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