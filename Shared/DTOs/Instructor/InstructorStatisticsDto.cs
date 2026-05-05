namespace ELProject.Shared.DTOs.Instructor
{
    public class InstructorStatisticsDto
    {
        public int CoursesCount { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}