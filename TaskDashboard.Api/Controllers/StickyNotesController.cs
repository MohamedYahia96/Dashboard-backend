using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.DTOs;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StickyNotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public StickyNotesController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StickyNoteDto>>> GetNotes()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notes = await _context.StickyNotes
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.ZIndex)
            .Select(n => new StickyNoteDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                Color = n.Color,
                PosX = n.PosX,
                PosY = n.PosY,
                ZIndex = n.ZIndex,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<StickyNoteDto>> CreateNote([FromBody] StickyNoteCreateDto createDto)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var note = new StickyNote
        {
            Title = createDto.Title,
            Content = createDto.Content,
            Color = createDto.Color,
            PosX = createDto.PosX,
            PosY = createDto.PosY,
            ZIndex = createDto.ZIndex,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.StickyNotes.Add(note);
        await _context.SaveChangesAsync();

        var result = new StickyNoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Color = note.Color,
            PosX = note.PosX,
            PosY = note.PosY,
            ZIndex = note.ZIndex,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };

        return CreatedAtAction(nameof(GetNotes), new { id = note.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StickyNoteDto>> UpdateNote(int id, [FromBody] StickyNoteUpdateDto updateDto)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var note = await _context.StickyNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (note == null) return NotFound();

        if (updateDto.Title != null) note.Title = updateDto.Title;
        if (updateDto.Content != null) note.Content = updateDto.Content;
        if (updateDto.Color != null) note.Color = updateDto.Color;
        if (updateDto.PosX.HasValue) note.PosX = updateDto.PosX.Value;
        if (updateDto.PosY.HasValue) note.PosY = updateDto.PosY.Value;
        if (updateDto.ZIndex.HasValue) note.ZIndex = updateDto.ZIndex.Value;

        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new StickyNoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Color = note.Color,
            PosX = note.PosX,
            PosY = note.PosY,
            ZIndex = note.ZIndex,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var note = await _context.StickyNotes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (note == null) return NotFound();

        _context.StickyNotes.Remove(note);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Note deleted" });
    }
}
