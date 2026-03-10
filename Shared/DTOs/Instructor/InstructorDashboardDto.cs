namespace ELProject.Shared.DTOs.Instructor
{
    public class InstructorDashboardDto
    {
        public int CoursesCount { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalRevenue { get; set; }
        public IReadOnlyList<InstructorCourseDto> Courses { get; set; } = new List<InstructorCourseDto>();
        public IReadOnlyList<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }

}
