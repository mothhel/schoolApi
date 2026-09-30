namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يمثل الشعبة عند العرض في الواجهة

        نستخدمه عندما نجلب الشعب من الـ API
        حتى نعرض:
        - رقم الشعبة
        - اسم الشعبة
        - الصف الذي تتبعه الشعبة
    */
    public class SectionViewModel
    {
        // رقم الشعبة في قاعدة البيانات
        public int Id { get; set; }

        // اسم الشعبة مثل: A أو B
        public string SectionName { get; set; } = string.Empty;

        // رقم الصف المرتبطة به الشعبة
        public int ClassRoomId { get; set; }

        // اسم الصف المرتبطة به الشعبة
        public string ClassRoomName { get; set; } = string.Empty;
    }
}