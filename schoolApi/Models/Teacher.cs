namespace schoolApi.Models
{
    /*
        هذا الكلاس يمثل جدول Teachers

        وظيفة هذا الجدول:
        تخزين البيانات التفصيلية الخاصة بالمدرس

        مثل:
        - رقم المدرس
        - التخصص
        - رقم الهاتف
        - العنوان
        - تاريخ التعيين

        بيانات الدخول الخاصة به موجودة في جدول Users
    */
    public class Teacher
    {
        public int Id { get; set; }

        // مفتاح خارجي يربط المدرس بحساب المستخدم الخاص به
        public int UserId { get; set; }

        // الربط مع جدول Users
        public User? User { get; set; }

        // الرقم الوظيفي أو رقم المدرس
        public string TeacherNumber { get; set; } = string.Empty;

        public string? Specialty { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }

        /*
            المدرس الواحد قد يدرس أكثر من مادة
            أو أكثر من صف أو شعبة
            لذلك نربطه مع TeacherAssignments
        */
        public ICollection<TeacherAssignment>? TeacherAssignments { get; set; }
    }
}
