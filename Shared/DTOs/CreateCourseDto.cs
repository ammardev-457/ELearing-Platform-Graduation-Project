using ELProject.Domain.Enums;

namespace ELProject.Shared.DTOs
{
    public class CreateCourseDto
    {
        public required string Title {get;set;}
        public decimal Price {get; set;}
        public int CategoryId {get; set;}
        // Seif Edition
        public CourseLevel? Level { get; set; } // Beginner, Intermediate, Advanced
    }
}