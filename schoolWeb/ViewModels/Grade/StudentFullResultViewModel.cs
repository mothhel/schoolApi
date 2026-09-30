using Microsoft.AspNetCore.Mvc.Rendering;

namespace schoolWeb.ViewModels.Grade
{
    public class StudentFullResultViewModel
    {
        public string StudentName { get; set; } = string.Empty;

        public string StudentNumber { get; set; } = string.Empty;

        public string ClassRoomName { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string SelectedClassRoomName { get; set; } = string.Empty;

        public List<string> AvailableClassRooms { get; set; } = new();

        public bool HasFinalYear { get; set; }


        public List<StudentResultSubjectViewModel> Results { get; set; } = new();

        public decimal GrandTotal { get; set; }

        public decimal MaxTotal { get; set; }

        public decimal Percentage { get; set; }

        public string Estimate { get; set; } = string.Empty;

        public string FinalStatus { get; set; } = string.Empty;

        public int FailedSubjectsCount { get; set; }

        public List<SelectListItem> Students { get; set; } = new();
    }
}