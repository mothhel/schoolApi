namespace schoolWeb.ViewModels.AdminSchoolSchedule
{
    /*
        هذا الكلاس يستخدم لعرض التكليفات في قائمة الاختيار

        التكليف يحتوي على:
        - المدرس
        - المادة
        - الصف
        - الشعبة
    */
    public class ScheduleAssignmentViewModel
    {
        public int Id { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;
    }
}