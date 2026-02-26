using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.DTOs;
using TaskDashboard.Api.Models;
using TaskDashboard.Api.Services;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ActivityLogService _activityLog;

    public TasksController(AppDbContext context, ActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult> GetTasks(
        [FromQuery] int? category,
        [FromQuery] int? tag,
        [FromQuery] Models.TaskStatus? status,
        [FromQuery] TaskPriority? priority,
        [FromQuery] bool? pinned)
    {
        var query = _context.TaskItems
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Category)
            .Include(t => t.Attachments)
            .Where(t => t.UserId == UserId);

        if (category.HasValue) query = query.Where(t => t.CategoryId == category);
        if (tag.HasValue) query = query.Where(t => t.TaskTags.Any(tt => tt.TagId == tag));
        if (status.HasValue) query = query.Where(t => t.Status == status);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority);
        if (pinned.HasValue) query = query.Where(t => t.IsPinned == pinned);

        var tasks = await query.OrderBy(t => t.Order).ThenByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetTask(int id)
    {
        var task = await _context.TaskItems
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult> CreateTask(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            RecurrenceType = dto.RecurrenceType,
            RecurrenceEndDate = dto.RecurrenceEndDate,
            UserId = UserId
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        // Add tags
        if (dto.TagIds?.Any() == true)
        {
            foreach (var tagId in dto.TagIds)
            {
                _context.TaskTags.Add(new TaskTag { TaskItemId = task.Id, TagId = tagId });
            }
            await _context.SaveChangesAsync();
        }

        await _activityLog.LogAsync(UserId, "Created", "Task", task.Id, $"Created task: {task.Title}");

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(int id, UpdateTaskDto dto)
    {
        var task = await _context.TaskItems
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

        if (task == null) return NotFound();

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status.HasValue)
        {
            task.Status = dto.Status.Value;
            if (dto.Status == Models.TaskStatus.Done)
                task.CompletedAt = DateTime.UtcNow;
        }
        if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
        if (dto.Progress.HasValue) task.Progress = Math.Clamp(dto.Progress.Value, 0, 100);
        if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;

        // Update tags
        if (dto.TagIds != null)
        {
            _context.TaskTags.RemoveRange(task.TaskTags);
            foreach (var tagId in dto.TagIds)
            {
                _context.TaskTags.Add(new TaskTag { TaskItemId = task.Id, TagId = tagId });
            }
        }

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(UserId, "Updated", "Task", task.Id, $"Updated task: {task.Title}");

        return Ok(task);
    }

    [HttpPut("{id}/pin")]
    public async Task<ActionResult> TogglePin(int id)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (task == null) return NotFound();

        task.IsPinned = !task.IsPinned;
        await _context.SaveChangesAsync();

        return Ok(task);
    }

    [HttpPut("reorder")]
    public async Task<ActionResult> Reorder(ReorderDto dto)
    {
        foreach (var item in dto.Items)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == item.Id && t.UserId == UserId);
            if (task != null) task.Order = item.Order;
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (task == null) return NotFound();

        await _activityLog.LogAsync(UserId, "Deleted", "Task", task.Id, $"Deleted task: {task.Title}");

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Task deleted" });
    }
}
