namespace schoolWeb.ViewModels.StudentPortal
{
    /*
        هذا الكلاس يستقبل عدادات الجديد من API
    */
    public class StudentPortalNewCountsViewModel
    {
        public int LessonsCount { get; set; }

        public int HomeworksCount { get; set; }

        public int AttendanceCount { get; set; }

        public int ScheduleCount { get; set; }

        public int NotesCount { get; set; }
    }
}