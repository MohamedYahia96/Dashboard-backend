using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.DTOs;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<ReportDto>> GetReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var tasks = await _context.TaskItems
            .Include(t => t.Category)
            .Where(t => t.UserId == UserId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToListAsync();

        var report = new ReportDto
        {
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == Models.TaskStatus.Done),
            InProgressTasks = tasks.Count(t => t.Status == Models.TaskStatus.InProgress),
            OverdueTasks = tasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != Models.TaskStatus.Done),
            CompletionRate = tasks.Count > 0
                ? Math.Round((double)tasks.Count(t => t.Status == Models.TaskStatus.Done) / tasks.Count * 100, 1)
                : 0
        };

        // Daily activity for chart
        var dailyGroups = tasks.GroupBy(t => t.CreatedAt.Date).OrderBy(g => g.Key);
        foreach (var group in dailyGroups)
        {
            report.DailyActivity.Add(new DailyActivityDto
            {
                Date = group.Key.ToString("yyyy-MM-dd"),
                Created = group.Count(),
                Completed = group.Count(t => t.Status == Models.TaskStatus.Done)
            });
        }

        // Category stats
        var categories = await _context.Categories.ToListAsync();
        foreach (var cat in categories)
        {
            var catTasks = tasks.Where(t => t.CategoryId == cat.Id).ToList();
            if (catTasks.Count == 0) continue;
            report.CategoryStats.Add(new CategoryStatsDto
            {
                Name = cat.Name,
                Color = cat.Color,
                Total = catTasks.Count,
                Completed = catTasks.Count(t => t.Status == Models.TaskStatus.Done),
                Progress = Math.Round((double)catTasks.Count(t => t.Status == Models.TaskStatus.Done) / catTasks.Count * 100, 1)
            });
        }

        return Ok(report);
    }
}
