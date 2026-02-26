namespace TaskDashboard.Api.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "en";
    public string PreferredTheme { get; set; } = "dark";
    public bool HasCompletedOnboarding { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredTheme { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Models.TaskPriority Priority { get; set; } = Models.TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public Models.RecurrenceType RecurrenceType { get; set; } = Models.RecurrenceType.None;
    public DateTime? RecurrenceEndDate { get; set; }
    public List<int>? TagIds { get; set; }
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Models.TaskStatus? Status { get; set; }
    public Models.TaskPriority? Priority { get; set; }
    public int? Progress { get; set; }
    public DateTime? DueDate { get; set; }
    public List<int>? TagIds { get; set; }
}

public class ReorderDto
{
    public List<ReorderItem> Items { get; set; } = new();
}

public class ReorderItem
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChangeRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class ReportDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double CompletionRate { get; set; }
    public List<DailyActivityDto> DailyActivity { get; set; } = new();
    public List<CategoryStatsDto> CategoryStats { get; set; } = new();
}

public class DailyActivityDto
{
    public string Date { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Completed { get; set; }
}

public class CategoryStatsDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public double Progress { get; set; }
}
