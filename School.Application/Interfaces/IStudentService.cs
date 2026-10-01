namespace school.Application.Interfaces
{
    public interface IStudentService
    {
        Task<object> GetMyProfileAsync(int userId);
        Task<object> GetMySubjectsAsync(int userId);
        Task<object> GetMyGradesAsync(int userId);
        Task<object> GetMyAttendancesAsync(int userId);
        Task<object> GetMyNotesAsync(int userId);
        Task<object> GetMyDailyTasksAsync(int userId);
        Task<object> MarkMyAreaAsViewedAsync(int userId, string areaKey);
        Task<object> GetMyNewCountsAsync(int userId);
        Task<object> GetMyGradeClassRoomsAsync(int userId);
        Task<object> GetMyAssessmentGradesAsync(int userId, int classRoomId, string assessmentType);
        Task<object> GetMyClassResultAsync(int userId, int classRoomId, string resultType);
    }
}