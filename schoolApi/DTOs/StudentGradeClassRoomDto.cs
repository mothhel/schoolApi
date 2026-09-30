namespace schoolApi.DTOs
{
    /*
        هذا DTO يستخدم لعرض الصفوف التي يوجد للطالب درجات فيها.

        الطالب يختار الصف من التطبيق.
        الشعبة يتم عرضها تلقائيًا حسب الدرجات المسجلة لذلك الصف.
    */
    public class StudentGradeClassRoomDto
    {
        public int ClassRoomId { get; set; }

        public string ClassRoomName { get; set; } = string.Empty;

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;
    }
}