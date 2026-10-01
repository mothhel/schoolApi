using School.Application.DTOs; // تأكد من مسار الـ DTOs عندك

namespace school.Application.Interfaces
{
    public interface IAuthService
    {
        Task<object> LoginAsync(LoginDto dto);
        Task<object?> GetUserByIdAsync(int id);
        Task<string> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<string> AdminResetPasswordAsync(int userId, string newPassword);
    }
}