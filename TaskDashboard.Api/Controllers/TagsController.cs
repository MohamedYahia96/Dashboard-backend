using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.DTOs;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagsController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult> GetTags()
    {
        var tags = await _context.Tags.Where(t => t.UserId == UserId).ToListAsync();
        return Ok(tags);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTag(CreateTagDto dto)
    {
        var tag = new Tag { Name = dto.Name, Color = dto.Color, UserId = UserId };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (tag == null) return NotFound();
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Tag deleted" });
    }
}
