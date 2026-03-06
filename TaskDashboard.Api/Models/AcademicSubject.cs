namespace TaskDashboard.Api.Models;

public enum AcademicSubjectStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class AcademicSubject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float Credits { get; set; } = 3;         // ساعات معتمدة
    public float? Grade { get; set; }               // 0–100
    public string? GradePoint { get; set; }         // A, B+, B, C+, C, D, F
    public AcademicSubjectStatus Status { get; set; } = AcademicSubjectStatus.NotStarted;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int SemesterId { get; set; }
    public AcademicSemester Semester { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}
