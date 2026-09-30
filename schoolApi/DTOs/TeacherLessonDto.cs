namespace schoolApi.DTOs
{
    /*
        هذا DTO يستخدم لعرض درس واحد في تطبيق الأستاذ.

        يعرض:
        - بيانات الدرس
        - تاريخ الدرس
        - وقت الإضافة
        - المادة
        - الصف
        - الشعبة
    */
    public class TeacherLessonDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime LessonDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedDayName { get; set; } = string.Empty;

        public string CreatedTime { get; set; } = string.Empty;

        public int TeacherAssignmentId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedDayName { get; set; } = string.Empty;

        public string UpdatedTime { get; set; } = string.Empty;
    }
}