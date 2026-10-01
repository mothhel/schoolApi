using School.Application.DTOs;

namespace school.Application.Interfaces
{
    public interface IAdminService
    {
        // 1. إدارة المستخدمين (الطلاب، المدرسين، المدراء)
        Task<object> UpdateUsernameAsync(int userId, UpdateUsernameDto dto);
        Task<object> CreateAdminAsync(CreateAdminDto dto);
        Task<object> CreateTeacherAsync(CreateTeacherDto dto);
        Task<object> CreateStudentAsync(CreateStudentDto dto);
        Task<object> GetUsersAsync(string? roleName);
        Task<object> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto);
        Task<object> DeleteUserAsync(int id);
        Task<object> GetStudentsAsync();
        Task<object> GetStudentByIdAsync(int id);
        Task<object> UpdateStudentAsync(int id, UpdateStudentDto dto);
        Task<object> DeleteStudentAsync(int id);
        Task<object> GetTeachersAsync();
        Task<object> GetTeacherByIdAsync(int id);
        Task<object> UpdateTeacherAsync(int id, UpdateTeacherDto dto);
        Task<object> DeleteTeacherAsync(int id);

        // 2. إدارة الصفوف والشعب والمواد
        Task<object> CreateClassRoomAsync(CreateClassRoomDto dto);
        Task<object> GetClassRoomsAsync();
        Task<object> UpdateClassRoomAsync(int id, UpdateClassRoomDto dto);
        Task<object> DeleteClassRoomAsync(int id);
        Task<object> CreateSectionAsync(CreateSectionDto dto);
        Task<object> GetSectionsAsync();
        Task<object> GetSectionsByClassRoomAsync(string classRoomName);
        Task<object> UpdateSectionAsync(int id, UpdateSectionDto dto);
        Task<object> DeleteSectionAsync(int id);
        Task<object> CreateSubjectAsync(CreateSubjectDto dto);
        Task<object> GetSubjectsAsync();
        Task<object> GetSubjectByIdAsync(int id);
        Task<object> UpdateSubjectAsync(int id, UpdateSubjectDto dto);
        Task<object> DeleteSubjectAsync(int id);

        // 3. إدارة التكليفات التدريسية
        Task<object> CreateTeacherAssignmentAsync(CreateTeacherAssignmentDto dto);
        Task<object> GetTeacherAssignmentsAsync();
        Task<object> GetTeacherAssignmentByIdAsync(int id);
        Task<object> GetTeacherAssignmentsByTeacherAsync(int teacherId);
        Task<object> UpdateTeacherAssignmentAsync(int id, UpdateTeacherAssignmentDto dto);
        Task<object> DeleteTeacherAssignmentAsync(int id);
        Task<object> GetAssignmentsByClassSectionAsync(int classRoomId, int sectionId);

        // 4. إدارة الدروس والواجبات
        Task<object> CreateLessonAsync(CreateLessonDto dto);
        Task<object> GetLessonsAsync();
        Task<object> GetLessonByIdAsync(int id);
        Task<object> GetLessonsByAssignmentAsync(int teacherAssignmentId);
        Task<object> UpdateLessonAsync(int id, UpdateLessonDto dto);
        Task<object> DeleteLessonAsync(int id);
        Task<object> CreateHomeworkAsync(CreateHomeworkDto dto);
        Task<object> GetHomeworksAsync();
        Task<object> GetHomeworkByIdAsync(int id);
        Task<object> GetHomeworksByAssignmentAsync(int teacherAssignmentId);
        Task<object> UpdateHomeworkAsync(int id, UpdateHomeworkDto dto);
        Task<object> DeleteHomeworkAsync(int id);

        // 5. إدارة الحضور والغياب
        Task<object> CreateAttendanceAsync(CreateAttendanceDto dto);
        Task<object> GetAttendancesAsync();
        Task<object> GetAttendanceByIdAsync(int id);
        Task<object> GetAttendancesByStudentAsync(int studentId);
        Task<object> UpdateAttendanceAsync(int id, UpdateAttendanceDto dto);
        Task<object> DeleteAttendanceAsync(int id);
        Task<object> GetStudentAttendanceAsync(int studentId);

        // 6. إدارة الدرجات والنتائج
        Task<object> CreateMidYearGradeAsync(CreateMidYearGradeDto dto);
        Task<object> CreateFinalYearGradeAsync(CreateFinalYearGradeDto dto);
        Task<object> GetStudentGradesAsync(int studentId);
        Task<object> GetStudentFullResultAsync(string studentNumber);
        Task<object> UpdateGradeAsync(int id, UpdateGradeDto dto);
        Task<object> GetGradesAsync();
        Task<object> GetGradeByIdAsync(int id);
        Task<object> DeleteGradeAsync(int id);

        // 7. إدارة الملاحظات والمهام اليومية
        Task<object> CreateStudentNoteAsync(CreateStudentNoteDto dto);
        Task<object> GetStudentNotesAsync();
        Task<object> GetStudentNoteByIdAsync(int id);
        Task<object> GetStudentNotesByStudentAsync(int studentId);
        Task<object> UpdateStudentNoteAsync(int id, UpdateStudentNoteDto dto);
        Task<object> DeleteStudentNoteAsync(int id);
        Task<object> CreateDailyTaskAsync(CreateDailyTaskDto dto);
        Task<object> GetDailyTasksAsync();
        Task<object> GetDailyTaskByIdAsync(int id);
        Task<object> GetDailyTasksByStudentAsync(int studentId);
        Task<object> UpdateDailyTaskAsync(int id, UpdateDailyTaskDto dto);
        Task<object> DeleteDailyTaskAsync(int id);

        // 8. الجدول الدراسي والإحصائيات
        Task<object> GetDashboardStatsAsync();
        Task<object> GetSchoolSchedulesAsync();
        Task<object> GetSchoolSchedulesByClassSectionAsync(int classRoomId, int sectionId);
        Task<object> GetSchoolScheduleAsync(int id);
        Task<object> CreateSchoolScheduleAsync(CreateSchoolScheduleDto dto);
        Task<object> UpdateSchoolScheduleAsync(int id, UpdateSchoolScheduleDto dto);
        Task<object> DeleteSchoolScheduleAsync(int id);

        // 9. بوابة الطالب (Admin Tools)
        Task<object> GetStudentPortalNewCountsAsync(int studentId);
        Task<object> MarkStudentPortalAreaAsViewedAsync(int studentId, string areaKey);
    }
}