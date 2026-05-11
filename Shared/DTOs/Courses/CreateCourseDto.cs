using ELProject.Domain.Enums;
using ELProject.Shared.DTOs.Lessons;
using System.ComponentModel.DataAnnotations;

namespace ELProject.Shared.DTOs.Courses
{
    public class CreateCourseDto
    {
        public IFormFile? Thumbnail { get; set; } // Optional, can be uploaded later

        [Required]
        [MaxLength(200)]
        public required string Title {get;set;}

        [Range(0, 100000)]
        public decimal Price {get; set;}
        public int CategoryId {get; set;}
        public CourseLevel? Level { get; set; } // Beginner, Intermediate, Advanced
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
    }
}