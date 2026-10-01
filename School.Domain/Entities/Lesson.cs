namespace school.Domain.Entities
{
    /*
       هذا الكلاس يمثل جدول Lessons

       وظيفة هذا الجدول:
       تخزين الدروس التي يضيفها المدرس

       الدرس يكون مرتبطاً بتكليف تدريسي
       حتى نعرف لأي مادة وصف وشعبة هذا الدرس
   */
    public class Lesson
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }
        public TeacherAssignment? TeacherAssignment { get; set; }

        // عنوان الدرس
        public string Title { get; set; } = string.Empty;

        // شرح أو وصف الدرس
        public string? Description { get; set; }

        // تاريخ الدرس
        public DateTime LessonDate { get; set; }
        /*
            وقت إضافة الدرس الحقيقي.
        */
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        /*
    تاريخ ووقت آخر تعديل على الدرس.

    تكون null إذا لم يتم تعديل الدرس بعد.
*/
        public DateTime? UpdatedAt { get; set; }
    }
}
