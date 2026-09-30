namespace schoolWeb.ViewModels.Grade
{
    public class StudentResultSubjectViewModel
    {
        public string SubjectName { get; set; } = string.Empty;

        public decimal FirstAssessmentScore { get; set; }

        public decimal MidtermExamScore { get; set; }

        public decimal MidYearTotal { get; set; }

        public decimal SecondAssessmentScore { get; set; }

        public decimal FinalExamScore { get; set; }

        public decimal FinalYearTotal { get; set; }

        public decimal Total { get; set; }

        public bool IsFailed { get; set; }
    }
}