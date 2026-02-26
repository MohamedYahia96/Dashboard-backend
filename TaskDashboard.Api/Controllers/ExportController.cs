using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using TaskDashboard.Api.Data;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExportController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("csv")]
    public async Task<ActionResult> ExportCsv([FromQuery] int? category)
    {
        var query = _context.TaskItems
            .Include(t => t.Category)
            .Where(t => t.UserId == UserId);

        if (category.HasValue)
            query = query.Where(t => t.CategoryId == category);

        var tasks = await query.OrderBy(t => t.Category.Name).ThenBy(t => t.CreatedAt).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Title,Category,Status,Priority,Progress,DueDate,CreatedAt");
        foreach (var t in tasks)
        {
            csv.AppendLine($"\"{t.Title}\",\"{t.Category.Name}\",{t.Status},{t.Priority},{t.Progress}%,{t.DueDate?.ToString("yyyy-MM-dd") ?? ""},\"{t.CreatedAt:yyyy-MM-dd}\"");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "tasks.csv");
    }

    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories.OrderBy(c => c.Order).ToListAsync();
        return Ok(categories);
    }
}
