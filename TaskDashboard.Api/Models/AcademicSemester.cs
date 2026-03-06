namespace TaskDashboard.Api.Models;

public class AcademicSemester
{
    public int Id { get; set; }
    public int SemesterNumber { get; set; } // 1 or 2
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public ICollection<AcademicSubject> Subjects { get; set; } = new List<AcademicSubject>();
}
