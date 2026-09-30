namespace schoolApi.DTOs
{
    /*
        يمثل نتيجة الطالب الكاملة في صف محدد.

        resultType:
        - MidYear
        - FinalYear
    */
    public class StudentClassResultDto
    {
        public int ClassRoomId { get; set; }

        public string ClassRoomName { get; set; } = string.Empty;

        public int SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;

        public string ResultType { get; set; } = string.Empty;

        public string ResultTitle { get; set; } = string.Empty;

        public decimal TotalScore { get; set; }

        public decimal TotalMaxScore { get; set; }

        public decimal Percentage { get; set; }

        public string OverallStatus { get; set; } = string.Empty;

        public List<StudentResultSubjectDto> Subjects { get; set; } = new();
    }
}