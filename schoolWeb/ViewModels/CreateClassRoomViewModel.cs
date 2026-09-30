using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم عند إضافة صف جديد من الواجهة

        وظيفته:
        - استقبال اسم الصف من نموذج الإدخال
        - تطبيق التحقق Validation
    */
    public class CreateClassRoomViewModel
    {
        // اسم الصف مطلوب عند الإضافة
        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}