namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم لعرض بيانات الطلاب القادمة من الـ API

        نستخدمه في صفحة إدارة الطلاب لعرض:
        - رقم الطالب
        - اسم الطالب
        - اسم المستخدم
        - الصف
        - الشعبة
    */
    public class StudentViewModel
    {
        public int Id { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ParentName { get; set; }

        public string? ParentPhone { get; set; }
    }
}