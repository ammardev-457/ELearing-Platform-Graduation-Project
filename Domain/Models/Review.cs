using System.ComponentModel.DataAnnotations;

namespace ELProject.Domain.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string? Comment { get; set; }
        public int Rating { get; set; } // Range (1, 5)
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}