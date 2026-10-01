namespace school.Domain.Entities
{
    /*
        هذا الجدول يحفظ آخر عنصر شاهده الطالب في كل قسم من بوابة الطالب

        AreaKey أمثلة:
        - Lessons
        - Homeworks
        - Attendance
        - Schedule
        - Notes

        LastSeenId:
        آخر رقم عنصر شاهده الطالب في هذا القسم.
    */
    public class StudentPortalReadStatus
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string AreaKey { get; set; } = string.Empty;

        public int LastSeenId { get; set; }

        public DateTime LastViewedAt { get; set; } = DateTime.Now;

        public Student? Student { get; set; }
    }
}