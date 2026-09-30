namespace schoolWeb.ViewModels.UserManagement
{
    /*
        هذا الكلاس يستخدم لعرض المستخدمين في صفحة إدارة الحسابات

        المستخدم قد يكون:
        - مدير
        - مدرس
        - طالب

        لذلك نعرض:
        - الاسم
        - اسم المستخدم
        - نوع الحساب
        - حالة الحساب
        - رقم الطالب أو رقم المدرس إن وجد لاحقًا
    */
    public class UserViewModel
    {
        // رقم المستخدم في قاعدة البيانات
        public int Id { get; set; }

        // الاسم الكامل للمستخدم
        public string FullName { get; set; } = string.Empty;

        // اسم المستخدم الذي يدخل به للنظام
        public string Username { get; set; } = string.Empty;

        // رقم الدور في قاعدة البيانات
        public int RoleId { get; set; }

        // اسم الدور مثل: Admin / Teacher / Student
        public string RoleName { get; set; } = string.Empty;

        // هل الحساب مفعل أو معطل
        public bool IsActive { get; set; }

        // إذا كان المستخدم طالبًا سيظهر رقم الطالب
        public string? StudentNumber { get; set; }

        // إذا كان المستخدم مدرسًا سيظهر رقم المدرس
        public string? TeacherNumber { get; set; }
    }
}