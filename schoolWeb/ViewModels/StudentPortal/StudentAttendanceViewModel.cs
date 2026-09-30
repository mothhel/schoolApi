namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستخدم لعرض سجل الحضور والغياب الخاص بالطالب

        الطالب لا يضيف ولا يعدل ولا يحذف
        فقط يشاهد سجله.
    */
    public class StudentAttendanceViewModel
    {
        public int Id { get; set; }

        public DateTime AttendanceDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? SubjectName { get; set; }

        public string? ClassRoomName { get; set; }

        public string? SectionName { get; set; }

        public string? TeacherName { get; set; }
    }
}