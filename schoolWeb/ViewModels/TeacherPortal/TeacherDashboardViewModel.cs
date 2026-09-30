namespace schoolWeb.ViewModels.TeacherPortal
{
    /*
        هذا الكلاس يمثل لوحة المدرس الرئيسية

        يعرض:
        - بيانات المدرس
        - عدد التكليفات
        - المواد التي يدرسها
        - الصفوف والشعب المرتبطة به
    */
    public class TeacherDashboardViewModel
    {
        public int TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public string TeacherNumber { get; set; } = string.Empty;
        /*
    اسم اليوم الحالي بالعربي
    مثال: السبت / الأحد / الاثنين
*/
        public string TodayDayName { get; set; } = string.Empty;

        /*
            عدد الحصص المقررة للأستاذ في اليوم الحالي
        */
        public int TodayClassesCount { get; set; }

        /*
            هل اليوم الحالي هو يوم الجمعة؟
            إذا كان الجمعة نعرض إجازة.
        */
        public bool IsFridayHoliday { get; set; }

        /*
            قائمة التكليفات الخاصة بالمدرس
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
            عدد الشعب المكلف بها المدرس
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