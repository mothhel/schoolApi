namespace schoolApi.Models
{
    /*
       هذا الكلاس يمثل جدول StudentEnrollments

       وظيفة هذا الجدول:
       ربط الطالب بالمواد التي يدرسها فعلياً

       أي أنه يربط بين:
       - الطالب
       - التكليف التدريسي

       ومن خلال التكليف التدريسي نستطيع معرفة:
       - المادة
       - المدرس
       - الصف
       - الشعبة

       هذا الجدول مهم جداً عند عرض جدول الطالب
   */
    public class StudentEnrollment
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // التكليف التدريسي
        public int TeacherAssignmentId { get; set; }
        public TeacherAssignment? TeacherAssignment { get; set; }
    }
}
