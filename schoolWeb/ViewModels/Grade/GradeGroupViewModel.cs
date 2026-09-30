namespace schoolWeb.ViewModels.Grade
{
    public class GradeGroupViewModel
    {
        public int FirstGradeId { get; set; }

        public int ExamGradeId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string TermName { get; set; } = string.Empty;

        public decimal AssessmentScore { get; set; }

        public decimal ExamScore { get; set; }

        public decimal Total => AssessmentScore + ExamScore;
    }
}