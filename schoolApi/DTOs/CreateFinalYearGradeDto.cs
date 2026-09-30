public class CreateFinalYearGradeDto
{
    public string StudentNumber { get; set; }
    public string SubjectName { get; set; }
    public string ClassRoomName { get; set; }
    public string SectionName { get; set; }

    public decimal SecondAssessmentScore { get; set; }
    public decimal FinalExamScore { get; set; }
}