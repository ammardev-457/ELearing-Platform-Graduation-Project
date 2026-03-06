using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELProject.Domain.Models;

namespace ELProject.DataAccess.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.ProfileImage)
                .HasMaxLength(500); 

            builder.Property(u => u.Gender)
                .HasMaxLength(20);

            builder.Property(u => u.JoinDate)
                .HasDefaultValueSql("GETUTCDATE()");

        }
    }
}
