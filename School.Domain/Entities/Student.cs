using System.Diagnostics;

namespace school.Domain.Entities
{
    /*
       هذا الكلاس يمثل جدول Students

       وظيفة هذا الجدول:
       تخزين البيانات التفصيلية الخاصة بالطالب

       هذا الجدول لا يحتوي على بيانات الدخول
       لأن بيانات الدخول موجودة في جدول Users

       هذا الجدول يحتوي على:
       - رقم الطالب
       - الجنس
       - تاريخ الميلاد
       - العنوان
       - رقم الهاتف
       - اسم ولي الأمر
       - هاتف ولي الأمر
       - الصف
       - الشعبة
   */
    public class Student
    {
        public int Id { get; set; }

        // مفتاح خارجي يربط الطالب بحساب المستخدم الخاص به
        public int UserId { get; set; }

        // الربط مع جدول Users
        public User? User { get; set; }

        // الرقم الدراسي للطالب
        public string StudentNumber { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        // اسم ولي الأمر
        public string? ParentName { get; set; }

        // رقم هاتف ولي الأمر
        public string? ParentPhone { get; set; }

        // الصف الدراسي
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        // الشعبة
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        /*
            الطالب الواحد يدرس عدة مواد
            لذلك نربطه مع جدول StudentEnrollments
        */
        public ICollection<StudentEnrollment>? StudentEnrollments { get; set; }

        /*
            الطالب الواحد يمكن أن يكون له عدة سجلات حضور وغياب
        */
        public ICollection<Attendance>? Attendances { get; set; }

        /*
            الطالب الواحد يمكن أن يكون له عدة درجات
        */
        public ICollection<Grade>? Grades { get; set; }

        /*
            الطالب الواحد يمكن أن يكون له عدة ملاحظات
        */
        public ICollection<StudentNote>? StudentNotes { get; set; }

        /*
            الطالب الواحد يمكن أن يكون له عدة مهام يومية
        */
        public ICollection<DailyTask>? DailyTasks { get; set; }
    }
}
