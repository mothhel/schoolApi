using System.ComponentModel.DataAnnotations;

namespace schoolApi.DTOs
{
    public class UpdateGradeDto
    {
        [Required]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required]
        public string SectionName { get; set; } = string.Empty;

        [Required]
        public string TermName { get; set; } = string.Empty;

        public decimal? FirstAssessmentScore { get; set; }

        public decimal? MidtermExamScore { get; set; }

        public decimal? SecondAssessmentScore { get; set; }

        public decimal? FinalExamScore { get; set; }
    }
}