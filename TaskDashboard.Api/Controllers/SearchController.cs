using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new { tasks = Array.Empty<object>(), categories = Array.Empty<object>() });

        var searchTerm = q.ToLower();

        var tasks = await _context.TaskItems
            .Include(t => t.Category)
            .Where(t => t.UserId == UserId &&
                (t.Title.ToLower().Contains(searchTerm) ||
                 (t.Description != null && t.Description.ToLower().Contains(searchTerm))))
            .Take(10)
            .Select(t => new { t.Id, t.Title, t.Status, t.Priority, Category = t.Category.Name })
            .ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.Name.ToLower().Contains(searchTerm) || c.NameAr.Contains(searchTerm))
            .Select(c => new { c.Id, c.Name, c.NameAr, c.Icon, c.Color })
            .ToListAsync();

        return Ok(new { tasks, categories });
    }
}
