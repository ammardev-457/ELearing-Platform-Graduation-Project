using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
   
    public class Payment
    {

        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int? CourseId { get; set; } 
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "egp";
        public string Gateway { get; set; } = null!; // "Stripe", "PayPal", etc.
        public string GatewayCheckoutSessionId { get; set; } = null!;
        public PaymentStatus Status { get; set; } =  PaymentStatus.Pending; // Pending, Succeeded, Failed, Refunded
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        public ApplicationUser User { get; set; } = null!;
        public Course Course { get; set; } = null!;

    }
    
}