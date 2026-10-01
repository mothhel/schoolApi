using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school.Application.Interfaces;
using System.Security.Claims;

namespace schoolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")] // حماية: مسموح للطلاب فقط
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // دالة مساعدة لاستخراج ID المستخدم من التوكن
        private int GetUserIdFromToken()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                throw new UnauthorizedAccessException("لم يتم العثور على بيانات المستخدم داخل التوكن.");
            return userId;
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try { return Ok(await _studentService.GetMyProfileAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-subjects")]
        public async Task<IActionResult> GetMySubjects()
        {
            try { return Ok(await _studentService.GetMySubjectsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-grades")]
        public async Task<IActionResult> GetMyGrades()
        {
            try { return Ok(await _studentService.GetMyGradesAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-attendances")]
        public async Task<IActionResult> GetMyAttendances()
        {
            try { return Ok(await _studentService.GetMyAttendancesAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-notes")]
        public async Task<IActionResult> GetMyNotes()
        {
            try { return Ok(await _studentService.GetMyNotesAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-daily-tasks")]
        public async Task<IActionResult> GetMyDailyTasks()
        {
            try { return Ok(await _studentService.GetMyDailyTasksAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("mark-my-area-as-viewed/{areaKey}")]
        public async Task<IActionResult> MarkMyAreaAsViewed(string areaKey)
        {
            if (string.IsNullOrWhiteSpace(areaKey)) return BadRequest("نوع القسم مطلوب.");
            try { return Ok(await _studentService.MarkMyAreaAsViewedAsync(GetUserIdFromToken(), areaKey)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-new-counts")]
        public async Task<IActionResult> GetMyNewCounts()
        {
            try { return Ok(await _studentService.GetMyNewCountsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-grade-classrooms")]
        public async Task<IActionResult> GetMyGradeClassRooms()
        {
            try { return Ok(await _studentService.GetMyGradeClassRoomsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-assessment-grades")]
        public async Task<IActionResult> GetMyAssessmentGrades(int classRoomId, string assessmentType)
        {
            if (classRoomId <= 0) return BadRequest("يجب اختيار الصف.");
            if (string.IsNullOrWhiteSpace(assessmentType) || (assessmentType != "First" && assessmentType != "Second"))
                return BadRequest("نوع المحصلة غير صحيح. استخدم First أو Second.");

            try { return Ok(await _studentService.GetMyAssessmentGradesAsync(GetUserIdFromToken(), classRoomId, assessmentType)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-class-result")]
        public async Task<IActionResult> GetMyClassResult(int classRoomId, string resultType)
        {
            if (classRoomId <= 0) return BadRequest("يجب اختيار الصف.");
            if (string.IsNullOrWhiteSpace(resultType) || (resultType != "MidYear" && resultType != "FinalYear"))
                return BadRequest("نوع النتيجة غير صحيح. استخدم MidYear أو FinalYear.");

            try { return Ok(await _studentService.GetMyClassResultAsync(GetUserIdFromToken(), classRoomId, resultType)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}