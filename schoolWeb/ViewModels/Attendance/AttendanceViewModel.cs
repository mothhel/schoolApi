namespace schoolWeb.ViewModels.Attendance
{
    /*
        هذا الكلاس يستخدم لعرض سجلات الحضور والغياب
    */
    public class AttendanceViewModel
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string? StudentName { get; set; }

        public string? StudentNumber { get; set; }

        public int TeacherAssignmentId { get; set; }

        public string? TeacherName { get; set; }

        public string? TeacherNumber { get; set; }

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }

        public DateTime AttendanceDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}