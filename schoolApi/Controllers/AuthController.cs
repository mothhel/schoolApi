using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school.Application.Interfaces;
using School.Application.DTOs;
using System.Security.Claims;

namespace schoolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // حقن الواجهة الجديدة بدلاً من الـ DbContext
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound("المستخدم غير موجود.");

            return Ok(user);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // الـ Controller هو المسؤول عن استخراج بيانات الـ HTTP (مثل التوكن)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            try
            {
                var message = await _authService.ChangePasswordAsync(userId, dto);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("admin-reset-password/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminResetPassword(int userId, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword)) return BadRequest("كلمة المرور الجديدة مطلوبة.");

            try
            {
                var message = await _authService.AdminResetPasswordAsync(userId, newPassword);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}