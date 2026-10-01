using School.Application.DTOs;

namespace school.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<object> GetMyProfileAsync(int userId);
        Task<object> GetMyAssignmentsAsync(int userId);
        Task<object> GetMyLessonsAsync(int userId);
        Task<object> GetMyHomeworksAsync(int userId);
        Task<object> GetMyStudentsAsync(int userId);
        Task<object> GetMyStudentsGradesAsync(int userId);
        Task<object> GetMyStudentsAttendancesAsync(int userId);
        Task<object> GetMyScheduleAsync(int userId);

        Task<object> CreateLessonAsync(int userId, CreateLessonDto dto);
        Task<object> CreateHomeworkAsync(int userId, CreateHomeworkDto dto);
        Task<object> CreateAttendanceAsync(int userId, TeacherCreateAttendanceDto dto);
        Task<object> CreateGradeAsync(int userId, TeacherCreateGradeDto dto);

        Task<object> UpdateLessonAsync(int userId, int id, UpdateLessonDto dto);
        Task<object> UpdateHomeworkAsync(int userId, int id, UpdateHomeworkDto dto);
        Task<object> UpdateAttendanceAsync(int userId, int id, TeacherUpdateAttendanceDto dto);
        Task<object> UpdateGradeAsync(int userId, int id, TeacherUpdateGradeDto dto);

        Task<object> DeleteLessonAsync(int userId, int id);
        Task<object> DeleteHomeworkAsync(int userId, int id);
        Task<object> DeleteAttendanceAsync(int userId, int id);
        Task<object> DeleteGradeAsync(int userId, int id);
    }
}