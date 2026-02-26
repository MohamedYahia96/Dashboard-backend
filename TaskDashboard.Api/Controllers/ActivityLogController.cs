using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/activity-log")]
[Authorize]
public class ActivityLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActivityLogController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult> GetActivityLog([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.ActivityLogs.Where(a => a.UserId == UserId);

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { logs, total, page, pageSize });
    }
}
