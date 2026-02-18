using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class StudentQuizConfiguration : IEntityTypeConfiguration<StudentQuiz>
    {
        public void Configure(EntityTypeBuilder<StudentQuiz> builder)
        {
            // Composite Key
            builder.HasKey(sq => new { sq.UserId, sq.QuizId });

            builder.Property(sq => sq.Score)
                .IsRequired();

            builder.Property(sq => sq.SubmitDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(sq => sq.User)
                .WithMany(u => u.StudentQuizzes)
                .HasForeignKey(sq => sq.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sq => sq.Quiz)
                .WithMany(q => q.StudentQuizzes)
                .HasForeignKey(sq => sq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}