using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(q => q.TotalMarks)
                .IsRequired();

            builder.Property(q => q.TimeLimitInMinutes)
                .IsRequired();

            // Relationships
            builder.HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}