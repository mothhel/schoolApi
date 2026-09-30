using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    /*
        هذا الكلاس يستخدم لتعديل ملاحظة طالب
    */
    public class UpdateStudentNoteDto
    {
        [Required(ErrorMessage = "رقم الطالب مطلوب")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "نص الملاحظة مطلوب")]
        public string NoteText { get; set; } = string.Empty;
    }
}