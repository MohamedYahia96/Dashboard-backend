using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/courses?trackId=X
    [HttpGet]
    public async Task<ActionResult> GetCourses([FromQuery] int? trackId, [FromQuery] CourseStatus? status, [FromQuery] CoursePlatform? platform)
    {
        var query = _context.Courses
            .Where(c => c.UserId == UserId);

        if (trackId.HasValue) query = query.Where(c => c.CourseTrackId == trackId.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (platform.HasValue) query = query.Where(c => c.Platform == platform.Value);

        var courses = await query
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.LastStudiedAt)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(courses);
    }

    // GET /api/courses/:id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCourse(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (course == null) return NotFound();
        return Ok(course);
    }

    // GET /api/courses/last-studied  — for Quick Access widget
    [HttpGet("last-studied")]
    public async Task<ActionResult> GetLastStudied()
    {
        var course = await _context.Courses
            .Where(c => c.UserId == UserId && c.LastStudiedAt != null && c.Status != CourseStatus.Completed)
            .Include(c => c.CourseTrack)
            .OrderByDescending(c => c.LastStudiedAt)
            .FirstOrDefaultAsync();

        if (course == null) return NoContent();
        return Ok(course);
    }

    // POST /api/courses
    [HttpPost]
    public async Task<ActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        // Verify the track belongs to the user
        var trackExists = await _context.CourseTracks.AnyAsync(ct => ct.Id == dto.CourseTrackId && ct.UserId == UserId);
        if (!trackExists) return BadRequest("Invalid track");

        var course = new Course
        {
            Title = dto.Title,
            Instructor = dto.Instructor,
            Url = dto.Url,
            Platform = dto.Platform,
            Status = dto.Status,
            TotalHours = dto.TotalHours,
            CompletedHours = dto.CompletedHours,
            Rating = dto.Rating,
            Notes = dto.Notes,
            TargetDate = dto.TargetDate,
            CourseTrackId = dto.CourseTrackId,
            UserId = UserId
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    // PUT /api/courses/:id
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (course == null) return NotFound();

        if (dto.Title != null) course.Title = dto.Title;
        if (dto.Instructor != null) course.Instructor = dto.Instructor;
        if (dto.Url != null) course.Url = dto.Url;
        if (dto.Platform.HasValue) course.Platform = dto.Platform.Value;
        if (dto.Status.HasValue)
        {
            course.Status = dto.Status.Value;
            if (dto.Status == CourseStatus.Completed)
                course.CompletedHours = course.TotalHours;
        }
        if (dto.TotalHours.HasValue) course.TotalHours = dto.TotalHours.Value;
        if (dto.CompletedHours.HasValue) course.CompletedHours = Math.Clamp(dto.CompletedHours.Value, 0, course.TotalHours > 0 ? course.TotalHours : float.MaxValue);
        if (dto.Rating.HasValue) course.Rating = dto.Rating.Value;
        if (dto.Notes != null) course.Notes = dto.Notes;
        if (dto.TargetDate.HasValue) course.TargetDate = dto.TargetDate.Value;

        await _context.SaveChangesAsync();
        return Ok(course);
    }

    // PUT /api/courses/:id/pin
    [HttpPut("{id}/pin")]
    public async Task<ActionResult> TogglePin(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (course == null) return NotFound();
        course.IsPinned = !course.IsPinned;
        await _context.SaveChangesAsync();
        return Ok(course);
    }

    // PUT /api/courses/:id/study  — called when user clicks "Start Studying"
    [HttpPut("{id}/study")]
    public async Task<ActionResult> MarkStudied(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (course == null) return NotFound();
        course.LastStudiedAt = DateTime.UtcNow;
        if (course.Status == CourseStatus.NotStarted)
            course.Status = CourseStatus.InProgress;
        await _context.SaveChangesAsync();
        return Ok(course);
    }

    // DELETE /api/courses/:id
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId);
        if (course == null) return NotFound();
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Course deleted" });
    }
}

// DTOs
public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Instructor { get; set; }
    public string? Url { get; set; }
    public CoursePlatform Platform { get; set; } = CoursePlatform.Other;
    public CourseStatus Status { get; set; } = CourseStatus.NotStarted;
    public float TotalHours { get; set; } = 0;
    public float CompletedHours { get; set; } = 0;
    public int? Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime? TargetDate { get; set; }
    public int CourseTrackId { get; set; }
}

public class UpdateCourseDto
{
    public string? Title { get; set; }
    public string? Instructor { get; set; }
    public string? Url { get; set; }
    public CoursePlatform? Platform { get; set; }
    public CourseStatus? Status { get; set; }
    public float? TotalHours { get; set; }
    public float? CompletedHours { get; set; }
    public int? Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime? TargetDate { get; set; }
}
