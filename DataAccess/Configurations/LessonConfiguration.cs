using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.HasKey(l => l.Id);

            // Properties
            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(l => l.Order)
                .IsRequired();

            builder.Ignore(l => l.IsFreePreview);

            // Enums
            builder.Property(l => l.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            builder.Property(l => l.FileUrl)
                .HasMaxLength(1000);

            builder.Property(l => l.DurationInSeconds)
                .IsRequired(false);

            // Relationships
            builder.HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional Quiz Relationship
            builder.HasOne(l => l.Quiz)
            .WithMany()
            .HasForeignKey(l => l.QuizId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}