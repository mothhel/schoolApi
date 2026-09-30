namespace schoolApi.Models
{
    /*
       هذا الكلاس يمثل جدول Subjects

       وظيفة هذا الجدول:
       تخزين المواد الدراسية

       أمثلة:
       رياضيات
       علوم
       لغة عربية
       انجليزي
   */
    public class Subject
    {
        public int Id { get; set; }

        // اسم المادة
        public string Name { get; set; } = string.Empty;

        /*
            المادة الواحدة يمكن أن تُدرس من أكثر من مدرس
            وفي أكثر من صف وشعبة
            لذلك نربطها مع TeacherAssignments
        */
        public ICollection<TeacherAssignment>? TeacherAssignments { get; set; }
    }
}
