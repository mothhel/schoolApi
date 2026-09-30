using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Subject
{
    /*
        هذا الكلاس يستخدم عند إضافة مادة جديدة
    */
    public class CreateSubjectViewModel
    {
        [Required(ErrorMessage = "اسم المادة مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}