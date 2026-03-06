namespace TaskDashboard.Api.Models;

public class University
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#6366f1";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
}
