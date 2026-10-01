using System.Diagnostics;

namespace school.Domain.Entities
{
    /*
       هذا الكلاس يمثل جدول TeacherAssignments

       هذا الجدول من أهم الجداول في النظام

       وظيفته:
       تحديد من يدرس ماذا؟ ولمن؟

       يعني يربط بين:
       - المدرس
       - المادة
       - الصف
       - الشعبة

       مثال:
       المدرس أحمد يدرس مادة الرياضيات
       للصف الأول
       الشعبة A
   */
    public class TeacherAssignment
    {
        public int Id { get; set; }

        // المدرس
        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        // المادة
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        // الصف
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        // الشعبة
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        /*
            من خلال هذا الجدول يمكن ربط الطلاب المسجلين بهذه المادة
        */
        public ICollection<StudentEnrollment>? StudentEnrollments { get; set; }

        /*
            المدرس يضيف الدروس لهذا التكليف
        */
        public ICollection<Lesson>? Lessons { get; set; }

        /*
            المدرس يضيف الواجبات لهذا التكليف
        */
        public ICollection<Homework>? Homeworks { get; set; }

        /*
            يمكن تسجيل الحضور والغياب بناءً على هذا التكليف
        */
        public ICollection<Attendance>? Attendances { get; set; }

        /*
            يمكن تسجيل النتائج بناءً على هذا التكليف
        */
        public ICollection<Grade>? Grades { get; set; }
    }
}
