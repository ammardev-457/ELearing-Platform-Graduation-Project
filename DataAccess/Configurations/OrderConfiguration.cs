using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;
using ELProject.Domain.Enums;  

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Primary Key
        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.PaymobOrderId)
               .IsUnique(false); 

        // Properties
        builder.Property(o => o.Amount)
               .IsRequired();

        builder.Property(o => o.Currency)
               .HasMaxLength(3)          
               .HasDefaultValue("EGP")
               .IsRequired();

        builder.Property(o => o.Status)
               .HasMaxLength(50)         
               .HasDefaultValue(PaymentStatus.Pending)  
               .IsRequired();

        builder.Property(o => o.PaymobOrderId)   
               .IsRequired(false);               

        builder.Property(o => o.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()")  
               .IsRequired();

        builder.Property(o => o.UpdatedAt)
               .HasDefaultValueSql("GETUTCDATE()")  
               .IsRequired(); 

        // Relationships
        builder.HasOne(o => o.Student)
               .WithMany() 
               .HasForeignKey(o => o.StudentId)
               .OnDelete(DeleteBehavior.Restrict);  

        builder.HasOne(o => o.Course)
               .WithMany(c => c.Orders)  
               .HasForeignKey(o => o.CourseId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);  
    }
}