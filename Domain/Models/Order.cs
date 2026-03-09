using System.ComponentModel.DataAnnotations.Schema;
using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Order
    {
        public long Id { get; set; }
        public string StudentId { get; set; } = null!;
        public int CourseId { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public long? PaymobOrderId { get; set; }
        public string Status { get; set; } = OrderStatus.Pending.ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Transaction> Transactions { get; set; } = [];
        public Course Course { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
        public Enrollment Enrollment { get; set; } = null!;
    }
}