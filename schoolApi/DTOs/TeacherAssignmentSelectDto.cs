namespace schoolApi.DTOs
{
    /*
        هذا DTO يستخدم في تطبيق Flutter
        عند إضافة درس جديد.

        الأستاذ يختار التكليف التدريسي من القائمة:
        المادة - الصف - الشعبة

        لكن الذي نرسله عند الحفظ هو Id.
    */
    public class TeacherAssignmentSelectDto
    {
        public int Id { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }
}