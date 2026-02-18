using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Relationships
            builder.HasMany(c => c.Courses)
                .WithOne(co => co.Category)
                .HasForeignKey(co => co.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}