using ELProject.Domain.Models;

public class Enrollment
{
    public int Id { get; set; } 
    public string StudentId { get; set; } = null!;
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; } = DateTime.UtcNow;
    public decimal Progress { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public long OrderId { get; set; } 

    // Navigation
    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Order Order { get; set; } = null!;
}