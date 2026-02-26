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
                .HasMaxLength(500); // رابط الصورة قد يكون طويلاً

            builder.Property(u => u.Gender)
                .HasMaxLength(20);

            builder.Property(u => u.JoinDate)
                .HasDefaultValueSql("GETUTCDATE()"); // ضمان القيمة الافتراضية في قاعدة البيانات

            // 2. Relationships (Inverse sides are configured in other classes)
        }
    }
}
