using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public CourseLevel? Level { get; set; } // Beginner, Intermediate, Advanced

        public decimal Price { get; set; }

        [NotMapped]
        public double Rate => Reviews.Count != 0 ? Reviews.Average(r => r.Rating) : 0;
        // Foreign Keys
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<Section> Sections { get; set; } = [];
        public ICollection<Quiz> Quizzes { get; set; } = [];
        public ICollection<Enrollment> Enrollments { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Order> Orders {get; set;} = [];
    }
}