namespace school.Domain.Entities
{
    /*
       هذا الكلاس يمثل جدول Users

       وظيفة هذا الجدول:
       تخزين بيانات تسجيل الدخول لكل مستخدم في النظام
       سواء كان:
       - مدير
       - مدرس
       - طالب

       هذا الجدول يحتوي على:
       - الاسم الكامل
       - اسم المستخدم
       - كلمة المرور
       - الدور

       ملاحظة:
       يفضل تخزين كلمة المرور بشكل مشفر لاحقاً
       لكن حالياً يمكن البدء بـ Password ثم لاحقاً نحولها إلى Hash
   */
    public class User
    {
        public int Id { get; set; }

        // الاسم الكامل للمستخدم
        public string FullName { get; set; } = string.Empty;

        // اسم المستخدم للدخول للنظام
        public string Username { get; set; } = string.Empty;

        // كلمة المرور
        public string Password { get; set; } = string.Empty;

        // مفتاح خارجي يربط المستخدم بالدور
        public int RoleId { get; set; }

        // الربط مع جدول Roles
        public Role? Role { get; set; }

        // هل الحساب مفعل أم لا
        public bool IsActive { get; set; } = true;

        /*
            علاقة One-to-One مع Student
            المستخدم إذا كان طالباً سيكون له سجل واحد فقط في جدول Students
        */
        public Student? Student { get; set; }

        /*
            علاقة One-to-One مع Teacher
            المستخدم إذا كان مدرساً سيكون له سجل واحد فقط في جدول Teachers
        */
        public Teacher? Teacher { get; set; }

        /*
    هل هذا المستخدم هو مدير النظام الرئيسي؟

    إذا true:
    يستطيع إدارة المستخدمين وتعديل بياناتهم.

    إذا false:
    حتى لو كان RoleName = Admin
    لا يستطيع الدخول إلى إدارة المستخدمين.
*/
        public bool IsSystemAdmin { get; set; } = false;
    }
}
