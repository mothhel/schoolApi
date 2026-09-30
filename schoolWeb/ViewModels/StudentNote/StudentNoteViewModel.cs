namespace schoolWeb.ViewModels.StudentNote
{
    /*
        هذا الكلاس يستخدم لعرض ملاحظات الطلاب في صفحة الإدارة

        يحتوي على:
        - بيانات الملاحظة
        - بيانات الطالب المرتبط بها
        - تاريخ إنشاء الملاحظة
    */
    public class StudentNoteViewModel
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public string NoteText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}