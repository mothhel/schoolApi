using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.StudentNote
{
    /*
        هذا الكلاس يستخدم في صفحة إضافة ملاحظة جديدة

        طريقة العمل:
        - المدير يختار الطالب من قائمة بحث
        - يكتب نص الملاحظة
        - يتم إرسال البيانات إلى AdminStudentNoteController
    */
    public class CreateStudentNoteViewModel
    {
        [Required(ErrorMessage = "الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "نص الملاحظة مطلوب")]
        public string NoteText { get; set; } = string.Empty;

        /*
            قائمة الطلاب تظهر في الواجهة
            وتستخدم مع Select2 للبحث بالاسم أو رقم الطالب
        */
        public List<SelectListItem> Students { get; set; } = new();
    }
}