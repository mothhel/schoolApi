namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يمثل صفحة تكليفات المدرس

        الصفحة تعرض:
        - بيانات المدرس
        - قائمة المواد والصفوف والشعب المكلف بها

        نستخدم هذا الكلاس في:
        Views/TeacherAssignment/Index.cshtml
    */
    public class TeacherAssignmentsPageViewModel
    {
        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public string TeacherNumber { get; set; } = string.Empty;

        /*
            قائمة التكليفات الخاصة بالمدرس
            كل تكليف يحتوي:
            - المادة
            - الصف
            - الشعبة
        */
        public List<TeacherPortalAssignmentViewModel> Assignments { get; set; } = new();

        /*
            عدد المواد المختلفة التي يدرسها المدرس
        */
        public int SubjectsCount => Assignments
            .Select(a => a.SubjectName)
            .Distinct()
            .Count();

        /*
            عدد الشعب المختلفة التي يدرسها المدرس
        */
        public int SectionsCount => Assignments
            .Select(a => $"{a.ClassRoomName}-{a.SectionName}")
            .Distinct()
            .Count();

        /*
            عدد التكليفات كاملة
        */
        public int AssignmentsCount => Assignments.Count;
    }
}