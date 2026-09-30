namespace schoolApi.Models
{
    /*
       هذا الكلاس يمثل جدول Attendance

       وظيفة هذا الجدول:
       تسجيل حضور وغياب الطالب

       كل سجل حضور مرتبط بـ:
       - طالب
       - مادة/مدرس/صف/شعبة عن طريق TeacherAssignment
       - تاريخ الحضور
   */
    public class Attendance
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // التكليف التدريسي
        public int TeacherAssignmentId { get; set; }
        public TeacherAssignment? TeacherAssignment { get; set; }

        // تاريخ تسجيل الحضور أو الغياب
        public DateTime AttendanceDate { get; set; }

        // الحالة: حاضر / غائب / متأخر
        public string Status { get; set; } = string.Empty;

        // ملاحظات إضافية
        public string? Notes { get; set; }
    }
}
