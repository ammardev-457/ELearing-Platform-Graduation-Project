using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => new { e.UserId, e.CourseId });

            builder.Property(e => e.EnrollDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.IsCompleted)
                .HasDefaultValue(false);

            builder.Property(e => e.Progress)
                .HasPrecision(5, 2); // 100.00 (5)

            builder.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Payment)
                .WithOne(p => p.Enrollment)
                .HasForeignKey<Enrollment>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}