namespace schoolApi.Models
{
    /*
      هذا الكلاس يمثل جدول DailyTasks

      وظيفة هذا الجدول:
      تخزين المهام اليومية الخاصة بالطالب

      مثل:
      - إحضار الدفتر
      - حل التمارين
      - مراجعة الدرس
  */
    public class DailyTask
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // عنوان المهمة
        public string Title { get; set; } = string.Empty;

        // وصف المهمة
        public string? Description { get; set; }

        // تاريخ المهمة
        public DateTime TaskDate { get; set; } = DateTime.Now;

        // هل تم إنجاز المهمة
        public bool IsCompleted { get; set; } = false;
    }
}
