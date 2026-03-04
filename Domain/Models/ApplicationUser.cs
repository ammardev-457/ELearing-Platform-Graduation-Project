using ELProject.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ELProject.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        // Navigation Properties
        public ICollection<Course> CreatedCourses { get; set; } = [];
        public ICollection<Enrollment> Enrollments { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];
        public ICollection<StudentQuiz> StudentQuizzes { get; set; } = [];
    }
}