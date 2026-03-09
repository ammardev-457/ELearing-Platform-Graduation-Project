
using ELProject.Domain.Enums;

namespace ELProject.Domain.Models
{
    public class Transaction
    {
        public long Id { get; set; }                  // معرف داخلي
        public long OrderId { get; set; }             // FK → Order.Id
        public string TransactionId { get; set; } = null!;    // Paymob transaction_id
        public long Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Status { get; set; } = TransactionStatus.Pending.ToString(); // pending / success / failed / refunded
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual Order Order { get; set; } = null!;
    }
}