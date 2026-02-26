namespace TaskDashboard.Api.Models;

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public enum RecurrenceType
{
    None,
    Daily,
    Weekly,
    Monthly
}

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int Progress { get; set; } = 0;
    public bool IsPinned { get; set; } = false;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Recurrence
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public DateTime? RecurrenceEndDate { get; set; }
    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }

    // Relations
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
