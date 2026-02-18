using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength(); // e.g. "USD", "EGP"

            builder.Property(p => p.Gateway)
                .IsRequired()
                .HasMaxLength(50); // "Stripe", "PayPal"

            builder.Property(p => p.GatewayCheckoutSessionId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Status)
                .HasConversion<string>() // Enum
                .HasMaxLength(20);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction); // الحفاظ على سجل المدفوعات

            builder.HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .IsRequired(false) // لأن الـ Property في الكلاس nullable
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}