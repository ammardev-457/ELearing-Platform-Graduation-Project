using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs.Courses
{
    public class UpdateCourseDto
    {
        public IFormFile? Thumbnail { get; set; }
        public string Title { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public CourseLevel? Level { get; set; } // Beginner, Intermediate, Advanced
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
    }
}
