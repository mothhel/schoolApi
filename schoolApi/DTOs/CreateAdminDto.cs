namespace schoolApi.DTOs
{
    /*
       هذا الكلاس يستخدم لاستقبال بيانات إنشاء مدير جديد

       لماذا نستخدم DTO؟
       لأننا لا نريد استقبال كائن User كامل من الواجهة
       بل فقط الحقول التي نحتاجها أثناء عملية الإضافة
   */
    public class CreateAdminDto
    {
        // الاسم الكامل للمدير
        public string FullName { get; set; } = string.Empty;

        // اسم المستخدم للدخول
        public string Username { get; set; } = string.Empty;

        // كلمة المرور
        public string Password { get; set; } = string.Empty;
    }
}
