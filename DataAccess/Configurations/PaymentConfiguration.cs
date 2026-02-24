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

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength();

            builder.Property(p => p.Gateway)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.GatewayCheckoutSessionId)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(p => p.GatewayCheckoutSessionId)
                .IsUnique();

            builder.Property(p => p.PaymentIntentId)
                .HasMaxLength(255);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdateTime)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(p => p.Student)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict );

            builder.HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}