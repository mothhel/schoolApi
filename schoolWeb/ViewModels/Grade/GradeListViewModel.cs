namespace schoolWeb.ViewModels.Grade
{
    public class GradeListViewModel
    {
        public int Id { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string TermName { get; set; } = string.Empty;

        public string GradeType { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal MaxScore { get; set; }
    }
}