using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(q => q.CorrectAnswer)
                .IsRequired()
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}