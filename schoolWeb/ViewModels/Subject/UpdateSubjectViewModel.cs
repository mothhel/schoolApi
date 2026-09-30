using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Subject
{
    /*
        هذا الكلاس يستخدم عند تعديل مادة
    */
    public class UpdateSubjectViewModel
    {
        // رقم المادة
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المادة مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}