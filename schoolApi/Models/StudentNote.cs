namespace schoolApi.Models
{
    /*
       هذا الكلاس يمثل جدول StudentNotes

       وظيفة هذا الجدول:
       تخزين ملاحظات خاصة بالطالب

       أمثلة:
       - ممتاز في السلوك
       - يحتاج متابعة
       - كثير الغياب
   */
    public class StudentNote
    {
        public int Id { get; set; }

        // الطالب
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // نص الملاحظة
        public string NoteText { get; set; } = string.Empty;

        // تاريخ الملاحظة
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
