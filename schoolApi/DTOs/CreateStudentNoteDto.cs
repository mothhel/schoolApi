using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لإضافة ملاحظة جديدة لطالب
    */
    public class CreateStudentNoteDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "نص الملاحظة مطلوب")]
        public string NoteText { get; set; } = string.Empty;
    }
}