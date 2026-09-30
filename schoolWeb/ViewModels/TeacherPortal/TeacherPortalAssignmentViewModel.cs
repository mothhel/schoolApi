namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يمثل تكليفًا واحدًا للمدرس

        التكليف يعني:
        - المادة التي يدرسها
        - الصف
        - الشعبة

        مثال:
        رياضيات - الصف الأول - شعبة A
    */
    public class TeacherPortalAssignmentViewModel
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int ClassRoomId { get; set; }

        public string ClassRoomName { get; set; } = string.Empty;

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;
    }
}