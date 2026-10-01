namespace school.Domain.Entities
{
    /*
        هذا الكلاس يمثل جدول Homeworks

        وظيفة هذا الجدول:
        تخزين الواجبات التي يضيفها المدرس

        الواجب مرتبط بتكليف تدريسي
        حتى نعرف لأي مادة وصف وشعبة هذا الواجب
    */
    public class Homework
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }
        public TeacherAssignment? TeacherAssignment { get; set; }

        // عنوان الواجب
        public string Title { get; set; } = string.Empty;

        // وصف الواجب
        public string? Description { get; set; }

        // آخر موعد للتسليم
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
