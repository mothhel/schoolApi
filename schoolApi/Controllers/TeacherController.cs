using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school.Application.Interfaces;
using School.Application.DTOs;
using System.Security.Claims;

namespace schoolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
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
            try { return Ok(await _teacherService.GetMyProfileAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            try { return Ok(await _teacherService.GetMyAssignmentsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-lessons")]
        public async Task<IActionResult> GetMyLessons()
        {
            try { return Ok(await _teacherService.GetMyLessonsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-homeworks")]
        public async Task<IActionResult> GetMyHomeworks()
        {
            try { return Ok(await _teacherService.GetMyHomeworksAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-students")]
        public async Task<IActionResult> GetMyStudents()
        {
            try { return Ok(await _teacherService.GetMyStudentsAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-students-grades")]
        public async Task<IActionResult> GetMyStudentsGrades()
        {
            try { return Ok(await _teacherService.GetMyStudentsGradesAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-students-attendances")]
        public async Task<IActionResult> GetMyStudentsAttendances()
        {
            try { return Ok(await _teacherService.GetMyStudentsAttendancesAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule()
        {
            try { return Ok(await _teacherService.GetMyScheduleAsync(GetUserIdFromToken())); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson(CreateLessonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.CreateLessonAsync(GetUserIdFromToken(), dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create-homework")]
        public async Task<IActionResult> CreateHomework(CreateHomeworkDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.CreateHomeworkAsync(GetUserIdFromToken(), dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create-attendance")]
        public async Task<IActionResult> CreateAttendance(TeacherCreateAttendanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.CreateAttendanceAsync(GetUserIdFromToken(), dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create-grade")]
        public async Task<IActionResult> CreateGrade(TeacherCreateGradeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.CreateGradeAsync(GetUserIdFromToken(), dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("update-lesson/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.UpdateLessonAsync(GetUserIdFromToken(), id, dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("update-homework/{id}")]
        public async Task<IActionResult> UpdateHomework(int id, UpdateHomeworkDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.UpdateHomeworkAsync(GetUserIdFromToken(), id, dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("update-attendance/{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, TeacherUpdateAttendanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.UpdateAttendanceAsync(GetUserIdFromToken(), id, dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("update-grade/{id}")]
        public async Task<IActionResult> UpdateGrade(int id, TeacherUpdateGradeDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try { return Ok(await _teacherService.UpdateGradeAsync(GetUserIdFromToken(), id, dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("delete-lesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try { return Ok(await _teacherService.DeleteLessonAsync(GetUserIdFromToken(), id)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("delete-homework/{id}")]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            try { return Ok(await _teacherService.DeleteHomeworkAsync(GetUserIdFromToken(), id)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("delete-attendance/{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try { return Ok(await _teacherService.DeleteAttendanceAsync(GetUserIdFromToken(), id)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("delete-grade/{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try { return Ok(await _teacherService.DeleteGradeAsync(GetUserIdFromToken(), id)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}