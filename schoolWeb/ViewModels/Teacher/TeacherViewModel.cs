namespace schoolWeb.ViewModels.Teacher
{
    /*
        هذا الكلاس يستخدم لعرض بيانات المدرسين في صفحة إدارة المدرسين
    */
    public class TeacherViewModel
    {
        public int Id { get; set; }

        public string TeacherNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string? Specialty { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? HireDate { get; set; }
    }
}