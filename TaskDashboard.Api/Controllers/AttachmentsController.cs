using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AttachmentsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("{taskId}")]
    public async Task<ActionResult> Upload(int taskId, IFormFile file)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == UserId);
        if (task == null) return NotFound();
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            FileName = file.FileName,
            FilePath = uniqueName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            TaskItemId = taskId
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(attachment);
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult> GetAttachments(int taskId)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == UserId);
        if (task == null) return NotFound();

        var attachments = await _context.Attachments.Where(a => a.TaskItemId == taskId).ToListAsync();
        return Ok(attachments);
    }

    [HttpGet("download/{id}")]
    public async Task<ActionResult> Download(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.TaskItem)
            .FirstOrDefaultAsync(a => a.Id == id && a.TaskItem.UserId == UserId);

        if (attachment == null) return NotFound();

        var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.FilePath);
        if (!System.IO.File.Exists(filePath)) return NotFound("File not found");

        return PhysicalFile(filePath, attachment.ContentType, attachment.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.TaskItem)
            .FirstOrDefaultAsync(a => a.Id == id && a.TaskItem.UserId == UserId);

        if (attachment == null) return NotFound();

        var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.FilePath);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Attachment deleted" });
    }
}
