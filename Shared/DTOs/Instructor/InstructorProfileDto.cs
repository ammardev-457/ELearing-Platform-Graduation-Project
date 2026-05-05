using ELProject.Domain.Models;

namespace ELProject.Shared.DTOs.Instructor
{
    public class InstructorProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PathOfImage { get; set; } = string.Empty;
        public string AboutMe { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public string? Bio { get; set; }
        public int CoursesCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
    }
}
