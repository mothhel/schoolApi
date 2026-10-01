using Microsoft.AspNetCore.Mvc;
using school.Application.Interfaces;
using School.Application.DTOs;

namespace schoolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService) => _adminService = adminService;

        private async Task<IActionResult> Handle(Func<Task<object>> action)
        {
            try { return Ok(await action()); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("update-username/{userId}")] public Task<IActionResult> UpdateUsername(int userId, UpdateUsernameDto dto) => Handle(() => _adminService.UpdateUsernameAsync(userId, dto));
        [HttpPost("create-admin")] public Task<IActionResult> CreateAdmin(CreateAdminDto dto) => Handle(() => _adminService.CreateAdminAsync(dto));
        [HttpPost("create-teacher")] public Task<IActionResult> CreateTeacher(CreateTeacherDto dto) => Handle(() => _adminService.CreateTeacherAsync(dto));
        [HttpPost("create-student")] public Task<IActionResult> CreateStudent(CreateStudentDto dto) => Handle(() => _adminService.CreateStudentAsync(dto));
        [HttpGet("get-users")] public Task<IActionResult> GetUsers(string? roleName = "All") => Handle(() => _adminService.GetUsersAsync(roleName));
        [HttpPut("update-user-status/{id}")] public Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDto dto) => Handle(() => _adminService.UpdateUserStatusAsync(id, dto));
        [HttpDelete("delete-user/{id}")] public Task<IActionResult> DeleteUser(int id) => Handle(() => _adminService.DeleteUserAsync(id));

        [HttpGet("get-students")] public Task<IActionResult> GetStudents() => Handle(() => _adminService.GetStudentsAsync());
        [HttpGet("get-student/{id}")] public Task<IActionResult> GetStudentById(int id) => Handle(() => _adminService.GetStudentByIdAsync(id));
        [HttpPut("update-student/{id}")] public Task<IActionResult> UpdateStudent(int id, UpdateStudentDto dto) => Handle(() => _adminService.UpdateStudentAsync(id, dto));
        [HttpDelete("delete-student/{id}")] public Task<IActionResult> DeleteStudent(int id) => Handle(() => _adminService.DeleteStudentAsync(id));
        [HttpGet("get-teachers")] public Task<IActionResult> GetTeachers() => Handle(() => _adminService.GetTeachersAsync());
        [HttpGet("get-teacher/{id}")] public Task<IActionResult> GetTeacherById(int id) => Handle(() => _adminService.GetTeacherByIdAsync(id));
        [HttpPut("update-teacher/{id}")] public Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto dto) => Handle(() => _adminService.UpdateTeacherAsync(id, dto));
        [HttpDelete("delete-teacher/{id}")] public Task<IActionResult> DeleteTeacher(int id) => Handle(() => _adminService.DeleteTeacherAsync(id));

        [HttpPost("create-classroom")] public Task<IActionResult> CreateClassRoom(CreateClassRoomDto dto) => Handle(() => _adminService.CreateClassRoomAsync(dto));
        [HttpGet("get-classrooms")] public Task<IActionResult> GetClassRooms() => Handle(() => _adminService.GetClassRoomsAsync());
        [HttpPut("update-classroom/{id}")] public Task<IActionResult> UpdateClassRoom(int id, UpdateClassRoomDto dto) => Handle(() => _adminService.UpdateClassRoomAsync(id, dto));
        [HttpDelete("delete-classroom/{id}")] public Task<IActionResult> DeleteClassRoom(int id) => Handle(() => _adminService.DeleteClassRoomAsync(id));

        [HttpPost("create-section")] public Task<IActionResult> CreateSection(CreateSectionDto dto) => Handle(() => _adminService.CreateSectionAsync(dto));
        [HttpGet("get-sections")] public Task<IActionResult> GetSections() => Handle(() => _adminService.GetSectionsAsync());
        [HttpGet("get-sections-by-classroom/{classRoomName}")] public Task<IActionResult> GetSectionsByClassRoom(string classRoomName) => Handle(() => _adminService.GetSectionsByClassRoomAsync(classRoomName));
        [HttpPut("update-section/{id}")] public Task<IActionResult> UpdateSection(int id, UpdateSectionDto dto) => Handle(() => _adminService.UpdateSectionAsync(id, dto));
        [HttpDelete("delete-section/{id}")] public Task<IActionResult> DeleteSection(int id) => Handle(() => _adminService.DeleteSectionAsync(id));

        [HttpPost("create-subject")] public Task<IActionResult> CreateSubject(CreateSubjectDto dto) => Handle(() => _adminService.CreateSubjectAsync(dto));
        [HttpGet("get-subjects")] public Task<IActionResult> GetSubjects() => Handle(() => _adminService.GetSubjectsAsync());
        [HttpGet("get-subject/{id}")] public Task<IActionResult> GetSubjectById(int id) => Handle(() => _adminService.GetSubjectByIdAsync(id));
        [HttpPut("update-subject/{id}")] public Task<IActionResult> UpdateSubject(int id, UpdateSubjectDto dto) => Handle(() => _adminService.UpdateSubjectAsync(id, dto));
        [HttpDelete("delete-subject/{id}")] public Task<IActionResult> DeleteSubject(int id) => Handle(() => _adminService.DeleteSubjectAsync(id));

        [HttpPost("create-teacher-assignment")] public Task<IActionResult> CreateTeacherAssignment(CreateTeacherAssignmentDto dto) => Handle(() => _adminService.CreateTeacherAssignmentAsync(dto));
        [HttpGet("get-teacher-assignments")] public Task<IActionResult> GetTeacherAssignments() => Handle(() => _adminService.GetTeacherAssignmentsAsync());
        [HttpGet("get-teacher-assignment/{id}")] public Task<IActionResult> GetTeacherAssignmentById(int id) => Handle(() => _adminService.GetTeacherAssignmentByIdAsync(id));
        [HttpGet("get-teacher-assignments-by-teacher/{teacherId}")] public Task<IActionResult> GetTeacherAssignmentsByTeacher(int teacherId) => Handle(() => _adminService.GetTeacherAssignmentsByTeacherAsync(teacherId));
        [HttpPut("update-teacher-assignment/{id}")] public Task<IActionResult> UpdateTeacherAssignment(int id, UpdateTeacherAssignmentDto dto) => Handle(() => _adminService.UpdateTeacherAssignmentAsync(id, dto));
        [HttpDelete("delete-teacher-assignment/{id}")] public Task<IActionResult> DeleteTeacherAssignment(int id) => Handle(() => _adminService.DeleteTeacherAssignmentAsync(id));
        [HttpGet("get-assignments-by-class-section")] public Task<IActionResult> GetAssignmentsByClassSection(int classRoomId, int sectionId) => Handle(() => _adminService.GetAssignmentsByClassSectionAsync(classRoomId, sectionId));

        [HttpPost("create-lesson")] public Task<IActionResult> CreateLesson(CreateLessonDto dto) => Handle(() => _adminService.CreateLessonAsync(dto));
        [HttpGet("get-lessons")] public Task<IActionResult> GetLessons() => Handle(() => _adminService.GetLessonsAsync());
        [HttpGet("get-lesson/{id}")] public Task<IActionResult> GetLessonById(int id) => Handle(() => _adminService.GetLessonByIdAsync(id));
        [HttpGet("get-lessons-by-assignment/{teacherAssignmentId}")] public Task<IActionResult> GetLessonsByAssignment(int teacherAssignmentId) => Handle(() => _adminService.GetLessonsByAssignmentAsync(teacherAssignmentId));
        [HttpPut("update-lesson/{id}")] public Task<IActionResult> UpdateLesson(int id, UpdateLessonDto dto) => Handle(() => _adminService.UpdateLessonAsync(id, dto));
        [HttpDelete("delete-lesson/{id}")] public Task<IActionResult> DeleteLesson(int id) => Handle(() => _adminService.DeleteLessonAsync(id));

        [HttpPost("create-homework")] public Task<IActionResult> CreateHomework(CreateHomeworkDto dto) => Handle(() => _adminService.CreateHomeworkAsync(dto));
        [HttpGet("get-homeworks")] public Task<IActionResult> GetHomeworks() => Handle(() => _adminService.GetHomeworksAsync());
        [HttpGet("get-homework/{id}")] public Task<IActionResult> GetHomeworkById(int id) => Handle(() => _adminService.GetHomeworkByIdAsync(id));
        [HttpGet("get-homeworks-by-assignment/{teacherAssignmentId}")] public Task<IActionResult> GetHomeworksByAssignment(int teacherAssignmentId) => Handle(() => _adminService.GetHomeworksByAssignmentAsync(teacherAssignmentId));
        [HttpPut("update-homework/{id}")] public Task<IActionResult> UpdateHomework(int id, UpdateHomeworkDto dto) => Handle(() => _adminService.UpdateHomeworkAsync(id, dto));
        [HttpDelete("delete-homework/{id}")] public Task<IActionResult> DeleteHomework(int id) => Handle(() => _adminService.DeleteHomeworkAsync(id));

        [HttpPost("create-attendance")] public Task<IActionResult> CreateAttendance(CreateAttendanceDto dto) => Handle(() => _adminService.CreateAttendanceAsync(dto));
        [HttpGet("get-attendances")] public Task<IActionResult> GetAttendances() => Handle(() => _adminService.GetAttendancesAsync());
        [HttpGet("get-attendance/{id}")] public Task<IActionResult> GetAttendanceById(int id) => Handle(() => _adminService.GetAttendanceByIdAsync(id));
        [HttpGet("get-attendances-by-student/{studentId}")] public Task<IActionResult> GetAttendancesByStudent(int studentId) => Handle(() => _adminService.GetAttendancesByStudentAsync(studentId));
        [HttpGet("get-student-attendance/{studentId}")] public Task<IActionResult> GetStudentAttendance(int studentId) => Handle(() => _adminService.GetStudentAttendanceAsync(studentId));
        [HttpPut("update-attendance/{id}")] public Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto dto) => Handle(() => _adminService.UpdateAttendanceAsync(id, dto));
        [HttpDelete("delete-attendance/{id}")] public Task<IActionResult> DeleteAttendance(int id) => Handle(() => _adminService.DeleteAttendanceAsync(id));

        [HttpPost("create-midyear-grade")] public Task<IActionResult> CreateMidYearGrade(CreateMidYearGradeDto dto) => Handle(() => _adminService.CreateMidYearGradeAsync(dto));
        [HttpPost("create-finalyear-grade")] public Task<IActionResult> CreateFinalYearGrade(CreateFinalYearGradeDto dto) => Handle(() => _adminService.CreateFinalYearGradeAsync(dto));
        [HttpGet("get-student-grades/{studentId}")] public Task<IActionResult> GetStudentGrades(int studentId) => Handle(() => _adminService.GetStudentGradesAsync(studentId));
        [HttpGet("get-student-full-result/{studentNumber}")] public Task<IActionResult> GetStudentFullResult(string studentNumber) => Handle(() => _adminService.GetStudentFullResultAsync(studentNumber));
        [HttpPut("update-grade/{id}")] public Task<IActionResult> UpdateGrade(int id, UpdateGradeDto dto) => Handle(() => _adminService.UpdateGradeAsync(id, dto));
        [HttpGet("get-grades")] public Task<IActionResult> GetGrades() => Handle(() => _adminService.GetGradesAsync());
        [HttpGet("get-grade/{id}")] public Task<IActionResult> GetGradeById(int id) => Handle(() => _adminService.GetGradeByIdAsync(id));
        [HttpDelete("delete-grade/{id}")] public Task<IActionResult> DeleteGrade(int id) => Handle(() => _adminService.DeleteGradeAsync(id));

        [HttpPost("create-student-note")] public Task<IActionResult> CreateStudentNote(CreateStudentNoteDto dto) => Handle(() => _adminService.CreateStudentNoteAsync(dto));
        [HttpGet("get-student-notes")] public Task<IActionResult> GetStudentNotes() => Handle(() => _adminService.GetStudentNotesAsync());
        [HttpGet("get-student-note/{id}")] public Task<IActionResult> GetStudentNoteById(int id) => Handle(() => _adminService.GetStudentNoteByIdAsync(id));
        [HttpGet("get-student-notes-by-student/{studentId}")] public Task<IActionResult> GetStudentNotesByStudent(int studentId) => Handle(() => _adminService.GetStudentNotesByStudentAsync(studentId));
        [HttpPut("update-student-note/{id}")] public Task<IActionResult> UpdateStudentNote(int id, UpdateStudentNoteDto dto) => Handle(() => _adminService.UpdateStudentNoteAsync(id, dto));
        [HttpDelete("delete-student-note/{id}")] public Task<IActionResult> DeleteStudentNote(int id) => Handle(() => _adminService.DeleteStudentNoteAsync(id));

        [HttpPost("create-daily-task")] public Task<IActionResult> CreateDailyTask(CreateDailyTaskDto dto) => Handle(() => _adminService.CreateDailyTaskAsync(dto));
        [HttpGet("get-daily-tasks")] public Task<IActionResult> GetDailyTasks() => Handle(() => _adminService.GetDailyTasksAsync());
        [HttpGet("get-daily-task/{id}")] public Task<IActionResult> GetDailyTaskById(int id) => Handle(() => _adminService.GetDailyTaskByIdAsync(id));
        [HttpGet("get-daily-tasks-by-student/{studentId}")] public Task<IActionResult> GetDailyTasksByStudent(int studentId) => Handle(() => _adminService.GetDailyTasksByStudentAsync(studentId));
        [HttpPut("update-daily-task/{id}")] public Task<IActionResult> UpdateDailyTask(int id, UpdateDailyTaskDto dto) => Handle(() => _adminService.UpdateDailyTaskAsync(id, dto));
        [HttpDelete("delete-daily-task/{id}")] public Task<IActionResult> DeleteDailyTask(int id) => Handle(() => _adminService.DeleteDailyTaskAsync(id));

        [HttpGet("dashboard-stats")] public Task<IActionResult> GetDashboardStats() => Handle(() => _adminService.GetDashboardStatsAsync());

        [HttpPost("create-school-schedule")] public Task<IActionResult> CreateSchoolSchedule(CreateSchoolScheduleDto dto) => Handle(() => _adminService.CreateSchoolScheduleAsync(dto));
        [HttpGet("get-school-schedules")] public Task<IActionResult> GetSchoolSchedules() => Handle(() => _adminService.GetSchoolSchedulesAsync());
        [HttpGet("get-school-schedules-by-class-section")] public Task<IActionResult> GetSchoolSchedulesByClassSection(int classRoomId, int sectionId) => Handle(() => _adminService.GetSchoolSchedulesByClassSectionAsync(classRoomId, sectionId));
        [HttpGet("get-school-schedule/{id}")] public Task<IActionResult> GetSchoolSchedule(int id) => Handle(() => _adminService.GetSchoolScheduleAsync(id));
        [HttpPut("update-school-schedule/{id}")] public Task<IActionResult> UpdateSchoolSchedule(int id, UpdateSchoolScheduleDto dto) => Handle(() => _adminService.UpdateSchoolScheduleAsync(id, dto));
        [HttpDelete("delete-school-schedule/{id}")] public Task<IActionResult> DeleteSchoolSchedule(int id) => Handle(() => _adminService.DeleteSchoolScheduleAsync(id));

        [HttpGet("get-student-portal-new-counts/{studentId}")] public Task<IActionResult> GetStudentPortalNewCounts(int studentId) => Handle(() => _adminService.GetStudentPortalNewCountsAsync(studentId));
        [HttpPost("mark-student-portal-area-as-viewed/{studentId}/{areaKey}")] public Task<IActionResult> MarkStudentPortalAreaAsViewed(int studentId, string areaKey) => Handle(() => _adminService.MarkStudentPortalAreaAsViewedAsync(studentId, areaKey));
    }
}