using Microsoft.EntityFrameworkCore;
using school.Application.Interfaces;
using School.Application.DTOs;

namespace school.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<object> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Username.Trim().ToLower() == dto.Username.Trim().ToLower());

            if (user == null || user.Password != dto.Password)
            {
                throw new Exception("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            if (!user.IsActive)
            {
                throw new Exception("هذا الحساب غير مفعل.");
            }

            string roleName = user.Role != null ? user.Role.Name : "Unknown";
            var token = _jwtService.GenerateToken(user, roleName);

            return new
            {
                message = "تم تسجيل الدخول بنجاح",
                token = token,
                userId = user.Id,
                fullName = user.FullName,
                username = user.Username,
                roleId = user.RoleId,
                roleName = roleName,
                isActive = user.IsActive,
                isSystemAdmin = user.IsSystemAdmin,
                studentId = user.Student != null ? user.Student.Id : (int?)null,
                teacherId = user.Teacher != null ? user.Teacher.Id : (int?)null
            };
        }

        public async Task<object?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Username,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    u.IsActive,
                    StudentId = u.Student != null ? u.Student.Id : (int?)null,
                    TeacherId = u.Teacher != null ? u.Teacher.Id : (int?)null
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<string> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                throw new Exception("كلمة المرور الجديدة وتأكيدها غير متطابقين.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود.");
            }

            if (user.Password != dto.CurrentPassword)
            {
                throw new Exception("كلمة المرور الحالية غير صحيحة.");
            }

            if (user.Password == dto.NewPassword)
            {
                throw new Exception("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية.");
            }

            user.Password = dto.NewPassword;
            await _context.SaveChangesAsync();

            return "تم تغيير كلمة المرور بنجاح";
        }

        public async Task<string> AdminResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود.");
            }

            user.Password = newPassword.Trim();
            await _context.SaveChangesAsync();

            return "تم إعادة تعيين كلمة المرور بنجاح";
        }
    }
}