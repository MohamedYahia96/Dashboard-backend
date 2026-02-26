namespace TaskDashboard.Api.Models;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
}
