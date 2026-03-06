using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasIndex(o => new {o.StudentId, o.CourseId}).IsUnique();

        builder.Property(o => o.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasMaxLength(5)
            .HasDefaultValue("EGP");

        builder.Property(o => o.Amount)
            .IsRequired();

        builder.HasOne(o => o.Student) 
            .WithMany()
            .HasForeignKey(o => o.StudentId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(o => o.Course)
           .WithMany(c => c.Orders) // لو كلاس الكورس فيه Orders اكتب: .WithMany(c => c.Orders)
           .HasForeignKey(o => o.CourseId) // بنقوله استخدم ده تحديداً
           .IsRequired()
           .OnDelete(DeleteBehavior.Restrict);
    }
}
