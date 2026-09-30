using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.Grade
{
    public class UpdateGradeViewModel
    {
        public int Id { get; set; }

        public string StudentNumber { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string TermName { get; set; } = string.Empty;

        [Range(0, 20)]
        public decimal? FirstAssessmentScore { get; set; }

        [Range(0, 30)]
        public decimal? MidtermExamScore { get; set; }

        [Range(0, 20)]
        public decimal? SecondAssessmentScore { get; set; }

        [Range(0, 30)]
        public decimal? FinalExamScore { get; set; }
    }
}