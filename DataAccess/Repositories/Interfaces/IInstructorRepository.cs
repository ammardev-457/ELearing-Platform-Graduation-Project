using ELProject.Shared.DTOs.Instructor;

namespace ELProject.DataAccess.Repositories.Interfaces
{
    public interface IInstructorRepository
    {
        Task<InstructorDashboardDto> GetInstructorDashboardAsync(string instructorId);
        Task<IReadOnlyList<InstructorCourseDto>> GetInstructorCoursesAsync(string instructorId);
        Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(string instructorId, int count = 5);
        // Optional: CRUD helpers for courses if you want repo to manage them
        // Task<Course> CreateCourseAsync(Course course);
        // Task UpdateCourseAsync(Course course);
        // Task DeleteCourseAsync(int courseId);
    }
}
