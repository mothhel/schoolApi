public class CreateMidYearGradeDto
{
    public string StudentNumber { get; set; }
    public string SubjectName { get; set; }
    public string ClassRoomName { get; set; }
    public string SectionName { get; set; }

    public decimal FirstAssessmentScore { get; set; }
    public decimal MidtermExamScore { get; set; }
}