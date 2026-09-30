namespace schoolWeb.ViewModels.AdminSchoolSchedule
{
    /*
        هذا الكلاس يستخدم لعرض حصة واحدة في الجدول الدراسي

        يعرض:
        - اليوم
        - رقم الحصة
        - المادة
        - المدرس
        - الصف
        - الشعبة
        - الوقت
        - القاعة

        مهم:
        ClassRoomId و SectionId نحتاجهم في التعديل والفلترة
        لأن الأسماء وحدها لا تكفي عند تجهيز القوائم.
    */
    public class SchoolScheduleViewModel
    {
        public int Id { get; set; }

        public int TeacherAssignmentId { get; set; }

        public int ClassRoomId { get; set; }

        public int SectionId { get; set; }

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