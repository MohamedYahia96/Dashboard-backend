using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/faculties")]
[Authorize]
public class FacultiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public FacultiesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/faculties?universityId=X
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? universityId)
    {
        var query = _context.Faculties
            .Include(f => f.University)
            .Include(f => f.AcademicYears)
                .ThenInclude(y => y.Semesters)
                    .ThenInclude(s => s.Subjects)
            .Where(f => f.University.UserId == UserId);

        if (universityId.HasValue)
            query = query.Where(f => f.UniversityId == universityId.Value);

        var faculties = await query.OrderBy(f => f.CreatedAt).ToListAsync();
        return Ok(faculties);
    }

    // GET /api/faculties/:id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetOne(int id)
    {
        var faculty = await _context.Faculties
            .Include(f => f.University)
            .Include(f => f.AcademicYears)
                .ThenInclude(y => y.Semesters)
                    .ThenInclude(s => s.Subjects)
            .FirstOrDefaultAsync(f => f.Id == id && f.University.UserId == UserId);

        if (faculty == null) return NotFound();
        return Ok(faculty);
    }

    // POST /api/faculties
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] FacultyDto dto)
    {
        // Verify university belongs to the user
        var universityExists = await _context.Universities
            .AnyAsync(u => u.Id == dto.UniversityId && u.UserId == UserId);
        if (!universityExists) return BadRequest("Invalid university");

        var faculty = new Faculty
        {
            Name = dto.Name,
            Description = dto.Description,
            Stage = dto.Stage ?? AcademicStage.Bachelor,
            UniversityId = dto.UniversityId
        };
        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = faculty.Id }, faculty);
    }

    // PUT /api/faculties/:id
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] FacultyDto dto)
    {
        var faculty = await _context.Faculties
            .Include(f => f.University)
            .FirstOrDefaultAsync(f => f.Id == id && f.University.UserId == UserId);
        if (faculty == null) return NotFound();

        if (dto.Name != null) faculty.Name = dto.Name;
        if (dto.Description != null) faculty.Description = dto.Description;
        if (dto.Stage != null) faculty.Stage = dto.Stage.Value;

        await _context.SaveChangesAsync();
        return Ok(faculty);
    }

    // DELETE /api/faculties/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var faculty = await _context.Faculties
            .Include(f => f.University)
            .FirstOrDefaultAsync(f => f.Id == id && f.University.UserId == UserId);
        if (faculty == null) return NotFound();
        _context.Faculties.Remove(faculty);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Faculty deleted" });
    }
}

public class FacultyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AcademicStage? Stage { get; set; }
    public int UniversityId { get; set; }
}
