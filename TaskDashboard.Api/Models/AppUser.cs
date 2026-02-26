using Microsoft.AspNetCore.Identity;

namespace TaskDashboard.Api.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public string PreferredTheme { get; set; } = "dark";
    public bool HasCompletedOnboarding { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
