namespace ELProject.Domain.Models
{
    public class Enrollment
    {
        public int Id { get; set; } 
        
        public string UserId { get; set; } = null!;
        public int CourseId { get; set; }
        
        public DateTime EnrollDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; } // more useful for subscription (year sub), temp suspension, refund case
        public decimal Progress { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? PaymentId { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}