namespace schoolWeb.ViewModels.Grade
{
    /*
        هذا الكلاس يستقبل بيانات حفظ درجة نهاية العام من الواجهة
        لطالب واحد في مادة واحدة
    */
    public class FinalYearStudentGradeSaveDto
    {
        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public decimal SecondAssessmentScore { get; set; }

        public decimal FinalExamScore { get; set; }
    }
}