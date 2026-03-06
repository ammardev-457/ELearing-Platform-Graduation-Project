using ELProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionId)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(t => t.TransactionId).IsUnique();

        builder.Property(t => t.Status)
            .HasMaxLength(20);

        builder.Property(t => t.Currency)
            .HasMaxLength(5);

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Transactions)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


