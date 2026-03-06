namespace TaskDashboard.Api.Models;

public class AcademicYear
{
    public int Id { get; set; }
    public int YearNumber { get; set; } // 1, 2, 3, 4, ...
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    public ICollection<AcademicSemester> Semesters { get; set; } = new List<AcademicSemester>();
}
