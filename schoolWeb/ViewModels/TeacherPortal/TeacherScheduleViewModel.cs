namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يستخدم لعرض الجدول الدراسي للأستاذ

        الأستاذ يرى فقط الحصص المرتبطة بتكليفاته هو.
    */
    public class TeacherScheduleViewModel
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string DayName { get; set; } = string.Empty;

        public int PeriodNumber { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        public string? RoomName { get; set; }

        public string? Notes { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;
    }
}