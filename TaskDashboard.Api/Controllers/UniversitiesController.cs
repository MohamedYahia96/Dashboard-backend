using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/universities")]
[Authorize]
public class UniversitiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public UniversitiesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/universities
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var universities = await _context.Universities
            .Where(u => u.UserId == UserId)
            .Include(u => u.Faculties)
                .ThenInclude(f => f.AcademicYears)
                    .ThenInclude(y => y.Semesters)
                        .ThenInclude(s => s.Subjects)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return Ok(universities);
    }

    // GET /api/universities/:id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetOne(int id)
    {
        var university = await _context.Universities
            .Include(u => u.Faculties)
                .ThenInclude(f => f.AcademicYears)
                    .ThenInclude(y => y.Semesters)
                        .ThenInclude(s => s.Subjects)
            .FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);

        if (university == null) return NotFound();
        return Ok(university);
    }

    // POST /api/universities
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] UniversityDto dto)
    {
        var university = new University
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color ?? "#6366f1",
            UserId = UserId
        };
        _context.Universities.Add(university);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = university.Id }, university);
    }

    // PUT /api/universities/:id
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UniversityDto dto)
    {
        var university = await _context.Universities.FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
        if (university == null) return NotFound();

        if (dto.Name != null) university.Name = dto.Name;
        if (dto.Description != null) university.Description = dto.Description;
        if (dto.Color != null) university.Color = dto.Color;

        await _context.SaveChangesAsync();
        return Ok(university);
    }

    // DELETE /api/universities/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var university = await _context.Universities.FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
        if (university == null) return NotFound();
        _context.Universities.Remove(university);
        await _context.SaveChangesAsync();
        return Ok(new { message = "University deleted" });
    }
}

public class UniversityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
}
