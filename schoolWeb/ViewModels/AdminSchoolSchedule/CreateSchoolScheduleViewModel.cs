using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels.AdminSchoolSchedule
{
    /*
        هذا الكلاس يستخدم في صفحة إضافة حصة للجدول الدراسي

        طريقة الإضافة:
        - المدير يختار الصف
        - يختار الشعبة
        - يختار التكليف التدريسي
        - يحدد اليوم ورقم الحصة والوقت
    */
    public class CreateSchoolScheduleViewModel
    {
        [Required(ErrorMessage = "الصف مطلوب")]
        public int ClassRoomId { get; set; }

        [Required(ErrorMessage = "الشعبة مطلوبة")]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "التكليف التدريسي مطلوب")]
        public int TeacherAssignmentId { get; set; }

        [Required(ErrorMessage = "اليوم مطلوب")]
        public string DayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الحصة مطلوب")]
        public int PeriodNumber { get; set; }

        public string? StartTime { get; set; }

        public string? EndTime { get; set; }

        public string? RoomName { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> ClassRooms { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();

        public List<SelectListItem> TeacherAssignments { get; set; } = new();

        public List<SelectListItem> Days { get; set; } = new();

        public List<SelectListItem> Periods { get; set; } = new();
    }
}