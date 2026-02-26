namespace TaskDashboard.Api.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}

public class TaskTag
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
