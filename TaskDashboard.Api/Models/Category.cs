namespace TaskDashboard.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public int Order { get; set; }
    public bool IsSystem { get; set; } = true;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
