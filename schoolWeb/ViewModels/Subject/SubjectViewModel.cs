namespace schoolWeb.ViewModels.Subject
{
    /*
        هذا الكلاس يستخدم لعرض المواد الدراسية في الواجهة
    */
    public class SubjectViewModel
    {
        // رقم المادة في قاعدة البيانات
        public int Id { get; set; }

        // اسم المادة مثل: رياضيات - علوم - عربي
        public string Name { get; set; } = string.Empty;
    }
}