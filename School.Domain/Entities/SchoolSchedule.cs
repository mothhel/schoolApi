namespace school.Domain.Entities
{
    /*
        هذا الموديل يمثل حصة واحدة في الجدول الدراسي

        الجدول مرتبط بتكليف تدريسي TeacherAssignment
        لأن التكليف يحتوي على:
        - المدرس
        - المادة
        - الصف
        - الشعبة

        مثال:
        يوم الأحد - الحصة الأولى - رياضيات - الصف الأول - شعبة A
    */
    public class SchoolSchedule
    {
        public int Id { get; set; }

        /*
            التكليف التدريسي:
            منه نعرف المادة والمدرس والصف والشعبة
        */
        public int TeacherAssignmentId { get; set; }

        /*
            اليوم:
            السبت / الأحد / الاثنين ...
        */
        public string DayName { get; set; } = string.Empty;

        /*
            رقم الحصة:
            1 / 2 / 3 / 4 ...
        */
        public int PeriodNumber { get; set; }

        /*
            وقت البداية والنهاية اختياري
            وضعناه string لتسهيل التعامل مع input type="time"
        */
        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        /*
            اسم القاعة أو الفصل اختياري
        */
        public string? RoomName { get; set; }

        /*
            ملاحظات اختيارية
        */
        public string? Notes { get; set; }

        public TeacherAssignment? TeacherAssignment { get; set; }
    }
}