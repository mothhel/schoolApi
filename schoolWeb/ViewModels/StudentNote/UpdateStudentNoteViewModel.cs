using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.StudentNote
{
    /*
        هذا الكلاس يستخدم في صفحة تعديل ملاحظة طالب

        يحتوي على:
        - Id الملاحظة
        - الطالب المرتبط بالملاحظة
        - نص الملاحظة الجديد
    */
    public class UpdateStudentNoteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "نص الملاحظة مطلوب")]
        public string NoteText { get; set; } = string.Empty;

        /*
            قائمة الطلاب حتى يمكن تغيير الطالب المرتبط بالملاحظة إذا احتجنا
        */
        public List<SelectListItem> Students { get; set; } = new();
    }
}