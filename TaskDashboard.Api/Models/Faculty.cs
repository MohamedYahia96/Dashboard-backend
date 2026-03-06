namespace TaskDashboard.Api.Models;

public enum AcademicStage
{
    Bachelor, // بكالوريوس
    Diploma,  // دبلوم
    Master,   // ماجستير
    PhD       // دكتوراه
}

public class Faculty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AcademicStage Stage { get; set; } = AcademicStage.Bachelor;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UniversityId { get; set; }
    public University University { get; set; } = null!;

    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
}
