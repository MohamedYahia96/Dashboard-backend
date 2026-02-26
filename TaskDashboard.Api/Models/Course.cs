namespace TaskDashboard.Api.Models;

public enum CoursePlatform
{
    Udemy,
    YouTube,
    Coursera,
    Pluralsight,
    Other
}

public enum CourseStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructor { get; set; }
    public string? Url { get; set; }
    public CoursePlatform Platform { get; set; } = CoursePlatform.Other;
    public CourseStatus Status { get; set; } = CourseStatus.NotStarted;
    public float TotalHours { get; set; } = 0;
    public float CompletedHours { get; set; } = 0;
    public int? Rating { get; set; }         // 1–5
    public string? Notes { get; set; }
    public bool IsPinned { get; set; } = false;
    public DateTime? TargetDate { get; set; }
    public DateTime? LastStudiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CourseTrackId { get; set; }
    public CourseTrack CourseTrack { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}
