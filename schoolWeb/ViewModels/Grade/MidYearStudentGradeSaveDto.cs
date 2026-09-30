namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يستقبل بيانات حفظ درجة نصف العام من الواجهة
        لطالب واحد في مادة واحدة
    */
    public class MidYearStudentGradeSaveDto
    {
        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public decimal FirstAssessmentScore { get; set; }

        public decimal MidtermExamScore { get; set; }
    }
}