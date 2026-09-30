namespace schoolApi.DTOs
{
    /*
        هذا DTO يستخدم لعرض حصة واحدة في جدول الأستاذ

        يعرض:
        - اليوم
        - رقم الحصة
        - الوقت
        - المادة
        - الصف
        - الشعبة
        - القاعة
        - الملاحظات
    */
    public class TeacherScheduleDto
    {
        public int Id { get; set; }

        public string DayName { get; set; } = string.Empty;

        public int PeriodNumber { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        public string? RoomName { get; set; }

        public string? Notes { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;
    }
}