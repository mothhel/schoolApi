namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يمثل الصف الدراسي عند العرض في الواجهة

        نستخدمه عندما نجلب الصفوف من الـ API
        حتى نعرض:
        - رقم الصف
        - اسم الصف
    */
    public class ClassRoomViewModel
    {
        // رقم الصف في قاعدة البيانات
        public int Id { get; set; }

        // اسم الصف مثل: الصف الأول - الصف الثاني
        public string Name { get; set; } = string.Empty;
    }
}