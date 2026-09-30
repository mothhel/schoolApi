namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستخدم لعرض ملاحظات الطالب
        ونستخدمه أيضًا لحساب عدد الملاحظات في لوحة الطالب.
    */
    public class StudentNoteViewModel
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string NoteText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}