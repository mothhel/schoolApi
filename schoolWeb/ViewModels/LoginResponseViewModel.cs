namespace schoolWeb.ViewModels
{
    public class LoginResponseViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        /*
            هل المستخدم مدير النظام الرئيسي؟
            هذه القيمة تأتي من API.
        */
        public bool IsSystemAdmin { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
    }
}