using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELProject.DataAccess.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(this IServiceProvider services)
        {
            // إنشاء Scope مؤقت لسحب الخدمات
            using var scope = services.CreateScope();
            
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            await SeedRolesAsync(roleManager);

            // 2. Seed Categories
            await SeedCategoriesAsync(context);
            
            // 3. (اختياري) Seed Admin User
            // await SeedAdminUserAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Technology"},
                    new() { Name = "Marketing" },
                    new() { Name = "Design"},
                    new() { Name = "AI" },
                    new() { Name = "Content Creation"},
                    new() { Name = "Personal Development"}
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}