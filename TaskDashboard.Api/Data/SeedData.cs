using Microsoft.AspNetCore.Identity;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Seed Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed Admin User
        var adminEmail = "admin@dashboard.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true,
                PreferredLanguage = "en",
                PreferredTheme = "dark"
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Courses", NameAr = "الكورسات", Icon = "GraduationCap", Color = "#6366f1", Order = 1 },
                new() { Name = "Entertainment & Sports", NameAr = "الترفيه والرياضة", Icon = "Gamepad2", Color = "#f43f5e", Order = 2 },
                new() { Name = "Study Sessions", NameAr = "جلسات المذاكرة", Icon = "BookOpen", Color = "#10b981", Order = 3 },
                new() { Name = "Social Media", NameAr = "مواقع السوشيال", Icon = "Share2", Color = "#3b82f6", Order = 4 },
                new() { Name = "Music", NameAr = "الموسيقى", Icon = "Music", Color = "#f59e0b", Order = 5 },
                new() { Name = "AI Tools", NameAr = "أدوات الذكاء الاصطناعي", Icon = "Bot", Color = "#8b5cf6", Order = 6 },
                new() { Name = "Academic Studies", NameAr = "الدراسة الأكاديمية", Icon = "School", Color = "#ec4899", Order = 7 },
                new() { Name = "Calendar & Appointments", NameAr = "المواعيد والتقويم", Icon = "Calendar", Color = "#14b8a6", Order = 8 }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
