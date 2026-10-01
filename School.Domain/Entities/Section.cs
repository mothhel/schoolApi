

namespace school.Domain.Entities
{
    /*
       هذا الكلاس يمثل جدول Sections

       وظيفة هذا الجدول:
       تخزين الشعب التابعة لكل صف

       مثال:
       الصف الأول -> شعبة A
       الصف الأول -> شعبة B

       كل شعبة تتبع صفاً واحداً فقط
   */
    public class Section
    {
        public int Id { get; set; }

        // اسم الشعبة
        public string Name { get; set; } = string.Empty;

        // مفتاح خارجي يشير إلى الصف
        public int ClassRoomId { get; set; }

        // الربط مع جدول الصفوف
        public ClassRoom? ClassRoom { get; set; }

        /*
            الشعبة الواحدة يمكن أن تضم عدداً كبيراً من الطلاب
        */
        public ICollection<Student>? Students { get; set; }

        /*
            الشعبة الواحدة يمكن أن يكون عليها أكثر من مدرس ومادة
        */
        public ICollection<TeacherAssignment>? TeacherAssignments { get; set; }
    }
}
