using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EnrollDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.IsCompleted)
                .HasDefaultValue(false);

            builder.Property(e => e.Progress)
                .HasPrecision(5, 2); 

            builder.HasIndex(e => e.OrderId)
                .IsUnique();

            builder.HasIndex(e => new {e.StudentId, e.CourseId})
                .IsUnique();
            
            
            builder.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Order)
                .WithOne(p => p.Enrollment)
                .HasForeignKey<Enrollment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}