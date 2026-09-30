using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace schoolWeb.ViewModels
{
    /*
        هذا الكلاس يستخدم لتعديل بيانات الطالب من الواجهة

        يحتوي على:
        - بيانات الحساب
        - بيانات الطالب
        - الصف والشعبة
        - قوائم Dropdown للصفوف والشعب
    */
    public class UpdateStudentViewModel
    {
        // رقم الطالب في قاعدة البيانات
        public int Id { get; set; }

        // رقم الطالب المدرسي مثل S1001
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الطالب مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        /*
    كلمة المرور الجديدة اختيارية
*/
        public string? Password { get; set; }

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

        // حالة الحساب
        public bool IsActive { get; set; } = true;

        // قائمة الصفوف
        public List<SelectListItem> ClassRooms { get; set; } = new();

        // قائمة الشعب
        public List<SelectListItem> Sections { get; set; } = new();
    }
}