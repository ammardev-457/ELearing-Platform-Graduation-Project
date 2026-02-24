using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "egp";
        public string Gateway { get; set; } = "Stripe";
        
        public string GatewayCheckoutSessionId { get; set; } = null!;
        
        public string? PaymentIntentId { get; set; }
        
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } // session maked moment
        public DateTime UpdateTime { get; set; } // webhook result moment

        public string StudentId { get; set; } = null!;
        public int CourseId { get; set; }

        public ApplicationUser Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        
        public Enrollment? Enrollment { get; set; }
    }
}