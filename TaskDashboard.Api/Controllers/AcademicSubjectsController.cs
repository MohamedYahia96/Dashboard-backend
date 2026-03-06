using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/academic-subjects")]
[Authorize]
public class AcademicSubjectsController : ControllerBase
{
    private readonly AppDbContext _context;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public AcademicSubjectsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/academic-subjects?semesterId=X
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? semesterId)
    {
        var query = _context.AcademicSubjects.Where(s => s.UserId == UserId);
        if (semesterId.HasValue)
            query = query.Where(s => s.SemesterId == semesterId.Value);

        var subjects = await query.OrderBy(s => s.CreatedAt).ToListAsync();
        return Ok(subjects);
    }

    // GET /api/academic-subjects/:id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetOne(int id)
    {
        var subject = await _context.AcademicSubjects
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (subject == null) return NotFound();
        return Ok(subject);
    }

    // POST /api/academic-subjects
    // Auto-creates AcademicYear and AcademicSemester if they don't exist
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSubjectDto dto)
    {
        // Verify faculty belongs to user
        var facultyValid = await _context.Faculties
            .Include(f => f.University)
            .AnyAsync(f => f.Id == dto.FacultyId && f.University.UserId == UserId);
        if (!facultyValid) return BadRequest("Invalid faculty");

        // Find or create AcademicYear
        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(y => y.FacultyId == dto.FacultyId && y.YearNumber == dto.YearNumber);
        if (academicYear == null)
        {
            academicYear = new AcademicYear { FacultyId = dto.FacultyId, YearNumber = dto.YearNumber };
            _context.AcademicYears.Add(academicYear);
            await _context.SaveChangesAsync();
        }

        // Find or create Semester
        var semester = await _context.AcademicSemesters
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYear.Id && s.SemesterNumber == dto.SemesterNumber);
        if (semester == null)
        {
            semester = new AcademicSemester { AcademicYearId = academicYear.Id, SemesterNumber = dto.SemesterNumber };
            _context.AcademicSemesters.Add(semester);
            await _context.SaveChangesAsync();
        }

        var subject = new AcademicSubject
        {
            Name = dto.Name,
            Credits = dto.Credits,
            Grade = dto.Grade,
            GradePoint = dto.Grade.HasValue ? CalculateGradePoint(dto.Grade.Value) : null,
            Status = dto.Status,
            Notes = dto.Notes,
            SemesterId = semester.Id,
            UserId = UserId
        };

        _context.AcademicSubjects.Add(subject);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = subject.Id }, subject);
    }

    // PUT /api/academic-subjects/:id
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSubjectDto dto)
    {
        var subject = await _context.AcademicSubjects
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (subject == null) return NotFound();

        if (dto.Name != null) subject.Name = dto.Name;
        if (dto.Credits.HasValue) subject.Credits = dto.Credits.Value;
        if (dto.Status.HasValue) subject.Status = dto.Status.Value;
        if (dto.Notes != null) subject.Notes = dto.Notes;
        if (dto.Grade.HasValue)
        {
            subject.Grade = dto.Grade.Value;
            subject.GradePoint = CalculateGradePoint(dto.Grade.Value);
        }

        await _context.SaveChangesAsync();
        return Ok(subject);
    }

    // DELETE /api/academic-subjects/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var subject = await _context.AcademicSubjects
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (subject == null) return NotFound();
        _context.AcademicSubjects.Remove(subject);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Subject deleted" });
    }

    private static string CalculateGradePoint(float grade)
    {
        return grade switch
        {
            >= 95 => "A+",
            >= 90 => "A",
            >= 85 => "B+",
            >= 80 => "B",
            >= 75 => "C+",
            >= 70 => "C",
            >= 65 => "D+",
            >= 60 => "D",
            _ => "F"
        };
    }
}

public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public float Credits { get; set; } = 3;
    public float? Grade { get; set; }
    public AcademicSubjectStatus Status { get; set; } = AcademicSubjectStatus.NotStarted;
    public string? Notes { get; set; }
    public int FacultyId { get; set; }
    public int YearNumber { get; set; }
    public int SemesterNumber { get; set; }
}

public class UpdateSubjectDto
{
    public string? Name { get; set; }
    public float? Credits { get; set; }
    public float? Grade { get; set; }
    public AcademicSubjectStatus? Status { get; set; }
    public string? Notes { get; set; }
}
