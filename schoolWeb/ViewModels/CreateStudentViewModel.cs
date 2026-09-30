using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم عند إضافة طالب جديد من الواجهة

        تم تحسينه بحيث يحتوي على:
        - بيانات الطالب
        - اسم الصف
        - اسم الشعبة
        - قائمة الصفوف Dropdown
        - قائمة الشعب Dropdown
    */
    public class CreateStudentViewModel
    {
        [Required(ErrorMessage = "اسم الطالب مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;

        //[Required(ErrorMessage = "رقم الطالب مطلوب")]
        //public string StudentNumber { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ParentName { get; set; }

        public string? ParentPhone { get; set; }

        [Required(ErrorMessage = "اسم الصف مطلوب")]
        public string ClassRoomName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الشعبة مطلوب")]
        public string SectionName { get; set; } = string.Empty;

        /*
            قائمة الصفوف التي ستظهر في Dropdown
        */
        public List<SelectListItem> ClassRooms { get; set; } = new();

        /*
            قائمة الشعب التي ستظهر في Dropdown
        */
        public List<SelectListItem> Sections { get; set; } = new();
    }
}