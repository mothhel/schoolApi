using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using schoolApi.Data;
using schoolApi.DTOs;
using schoolApi.Services;
using System.Security.Claims;

namespace schoolApi.Controllers
{
    /*
        هذا الكنترولر مسؤول عن تسجيل الدخول

        في هذه المرحلة سنقوم بعمل:
        - Login باستخدام JWT
        - التحقق من اسم المستخدم وكلمة المرور
        - تحديد نوع المستخدم
        - إعادة بيانات المستخدم الأساسية مع التوكن
    */

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SchoolDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(SchoolDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /*
    =====================================================
    69) API لتسجيل الدخول مع JWT Token
    =====================================================

    هذا الـ endpoint يقوم بالتحقق من:
    - اسم المستخدم
    - كلمة المرور

    ثم يعيد:
    - بيانات المستخدم
    - نوع المستخدم
    - هل هو مدير النظام الرئيسي
    - JWT Token
*/
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن المستخدم مع الدور والطالب والمدرس
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u =>
                    u.Username.Trim().ToLower() == dto.Username.Trim().ToLower());

            if (user == null)
            {
                return BadRequest("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // التحقق من أن الحساب مفعل
            if (!user.IsActive)
            {
                return BadRequest("هذا الحساب غير مفعل.");
            }

            // التحقق من كلمة المرور
            if (user.Password != dto.Password)
            {
                return BadRequest("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // تحديد نوع المستخدم
            string roleName = user.Role != null ? user.Role.Name : "Unknown";

            // إنشاء JWT Token
            var token = _jwtService.GenerateToken(user, roleName);

            return Ok(new
            {
                message = "تم تسجيل الدخول بنجاح",
                token = token,
                userId = user.Id,
                fullName = user.FullName,
                username = user.Username,
                roleId = user.RoleId,
                roleName = roleName,
                isActive = user.IsActive,

                /*
                    هل هذا المستخدم هو مدير النظام الرئيسي؟

                    إذا true:
                    تظهر له إدارة المستخدمين
                    ويستطيع تعديل بيانات المستخدمين.

                    إذا false:
                    حتى لو كان Admin
                    لا تظهر له إدارة المستخدمين.
                */
                isSystemAdmin = user.IsSystemAdmin,

                // إذا كان طالبًا نعيد Id الطالب
                studentId = user.Student != null ? user.Student.Id : (int?)null,

                // إذا كان مدرسًا نعيد Id المدرس
                teacherId = user.Teacher != null ? user.Teacher.Id : (int?)null
            });
        }

        /*
            =====================================================
            70) API لعرض بيانات المستخدم الحالي بواسطة Id
            =====================================================

            هذا الـ endpoint يعرض بيانات المستخدم الأساسية
            مع نوعه
        */
        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
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

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            return Ok(user);
        }


        /*
    =====================================================
    96) API لتغيير كلمة المرور للمستخدم الحالي
    =====================================================

    هذا الـ endpoint يسمح للمستخدم الحالي
    (مدير أو مدرس أو طالب)
    بتغيير كلمة المرور الخاصة به

    يتم التعرف على المستخدم من خلال الـ JWT Token
*/
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // التحقق من تطابق كلمة المرور الجديدة مع التأكيد
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest("كلمة المرور الجديدة وتأكيدها غير متطابقين.");
            }

            // استخراج userId من التوكن
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            }

            // البحث عن المستخدم
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            // التحقق من كلمة المرور الحالية
            if (user.Password != dto.CurrentPassword)
            {
                return BadRequest("كلمة المرور الحالية غير صحيحة.");
            }

            // منع استخدام نفس كلمة المرور القديمة
            if (user.Password == dto.NewPassword)
            {
                return BadRequest("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية.");
            }

            // تغيير كلمة المرور
            user.Password = dto.NewPassword;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تغيير كلمة المرور بنجاح"
            });
        }

        /*
            =====================================================
            97) API للمدير لتغيير كلمة مرور أي مستخدم
            =====================================================

            هذا الـ endpoint يسمح للمدير فقط
            بتغيير كلمة مرور أي مستخدم بواسطة userId

            يفيد هذا في حالة نسيان كلمة المرور
        */
        [HttpPost("admin-reset-password/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminResetPassword(int userId, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest("كلمة المرور الجديدة مطلوبة.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            user.Password = newPassword.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إعادة تعيين كلمة المرور بنجاح"
            });
        }
    }
}