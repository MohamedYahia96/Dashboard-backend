using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/course-tracks")]
[Authorize]
public class CourseTracksController : ControllerBase
{
    private readonly AppDbContext _context;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public CourseTracksController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/course-tracks
    [HttpGet]
    public async Task<ActionResult> GetTracks()
    {
        var tracks = await _context.CourseTracks
            .Where(ct => ct.UserId == UserId)
            .Include(ct => ct.Courses)
            .OrderBy(ct => ct.CreatedAt)
            .Select(ct => new
            {
                ct.Id,
                ct.Name,
                ct.Icon,
                ct.Color,
                ct.Description,
                ct.CreatedAt,
                TotalCourses = ct.Courses.Count,
                CompletedCourses = ct.Courses.Count(c => c.Status == CourseStatus.Completed),
                InProgressCourses = ct.Courses.Count(c => c.Status == CourseStatus.InProgress),
                TotalHours = ct.Courses.Sum(c => c.TotalHours),
                CompletedHours = ct.Courses.Sum(c => c.CompletedHours),
            })
            .ToListAsync();

        return Ok(tracks);
    }

    // GET /api/course-tracks/:id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetTrack(int id)
    {
        var track = await _context.CourseTracks
            .Where(ct => ct.Id == id && ct.UserId == UserId)
            .Include(ct => ct.Courses)
            .FirstOrDefaultAsync();

        if (track == null) return NotFound();
        return Ok(track);
    }

    // POST /api/course-tracks
    [HttpPost]
    public async Task<ActionResult> CreateTrack([FromBody] CreateCourseTrackDto dto)
    {
        var track = new CourseTrack
        {
            Name = dto.Name,
            Icon = dto.Icon ?? "BookOpen",
            Color = dto.Color ?? "#6366f1",
            Description = dto.Description,
            UserId = UserId
        };

        _context.CourseTracks.Add(track);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTrack), new { id = track.Id }, track);
    }

    // PUT /api/course-tracks/:id
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTrack(int id, [FromBody] CreateCourseTrackDto dto)
    {
        var track = await _context.CourseTracks.FirstOrDefaultAsync(ct => ct.Id == id && ct.UserId == UserId);
        if (track == null) return NotFound();

        track.Name = dto.Name;
        if (dto.Icon != null) track.Icon = dto.Icon;
        if (dto.Color != null) track.Color = dto.Color;
        track.Description = dto.Description;

        await _context.SaveChangesAsync();
        return Ok(track);
    }

    // DELETE /api/course-tracks/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTrack(int id)
    {
        var track = await _context.CourseTracks
            .Include(ct => ct.Courses)
            .FirstOrDefaultAsync(ct => ct.Id == id && ct.UserId == UserId);

        if (track == null) return NotFound();

        _context.CourseTracks.Remove(track); // cascades to Courses
        await _context.SaveChangesAsync();
        return Ok(new { message = "Track and its courses deleted" });
    }
}

// DTOs (inline for simplicity)
public class CreateCourseTrackDto
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}
