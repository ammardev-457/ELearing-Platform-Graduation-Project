using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // Composite Primary Key
            builder.HasKey(e => new { e.UserId, e.CourseId });

            // Properties
            builder.Property(e => e.EnrollDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.IsCompleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // حذف المستخدم يحذف اشتراكاته

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // حذف الكورس لا يجب أن يتم إذا كان هناك طلاب
        }
    }
}