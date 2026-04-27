using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.ShortDescription)
                .HasMaxLength(2000); // وصف طويل

            builder.Property(c => c.LongDescription)
                .HasMaxLength(2000); // وصف طويل

            builder.Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // تخزين الـ Enum كنص لسهولة القراءة في الداتابيز
            builder.Property(c => c.Level)
                .HasConversion<string>() 
                .HasMaxLength(50);

            builder.Property(c => c.Price)
                .HasColumnType("decimal(18,2)"); // مهم جداً للعملات

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Category)
                .WithMany(ca => ca.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}