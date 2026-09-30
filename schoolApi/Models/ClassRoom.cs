using static System.Collections.Specialized.BitVector32;

namespace schoolApi.Models
{
    /*
        هذا الكلاس يمثل جدول ClassRooms

        وظيفة هذا الجدول:
        تخزين الصفوف الدراسية

        أمثلة:
        الصف الأول
        الصف الثاني
        الصف الثالث

        استخدمنا اسم ClassRoom بدلاً من Class
        لأن Class كلمة محجوزة في C#
    */
    public class ClassRoom
    {
        public int Id { get; set; }

        // اسم الصف
        public string Name { get; set; } = string.Empty;

        /*
            الصف الواحد يمكن أن يحتوي على عدة شعب
            مثال:
            الصف الأول -> شعبة A و شعبة B
        */
        public ICollection<Section>? Sections { get; set; }

        /*
            الصف الواحد يمكن أن يحتوي على عدد كبير من الطلاب
        */
        public ICollection<Student>? Students { get; set; }

        /*
            الصف الواحد يمكن أن يكون مرتبطاً بعدة تكليفات تدريس
            أي أكثر من مدرس ومادة
        */
        public ICollection<TeacherAssignment>? TeacherAssignments { get; set; }
    }
}
