using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var query = _context.Appointments
            .Include(a => a.Reminders)
            .Where(a => a.UserId == UserId);

        if (start.HasValue)
            query = query.Where(a => a.EndDate >= start.Value);

        if (end.HasValue)
            query = query.Where(a => a.StartDate <= end.Value);

        var appointments = await query.ToListAsync();
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Reminders)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (appointment == null) return NotFound();

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto dto)
    {
        var appointment = new Appointment
        {
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Color = dto.Color ?? "#3b82f6",
            IsAllDay = dto.IsAllDay,
            UserId = UserId
        };

        if (dto.Reminders != null && dto.Reminders.Any())
        {
            foreach (var r in dto.Reminders)
            {
                appointment.Reminders.Add(new AppointmentReminder
                {
                    MinutesBefore = r.MinutesBefore,
                    ScheduledTime = appointment.StartDate.AddMinutes(-r.MinutesBefore)
                });
            }
        }

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Reminders)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (appointment == null) return NotFound();

        appointment.Title = dto.Title;
        appointment.Description = dto.Description;
        appointment.StartDate = dto.StartDate;
        appointment.EndDate = dto.EndDate;
        appointment.Color = dto.Color ?? "#3b82f6";
        appointment.IsAllDay = dto.IsAllDay;

        // Update Reminders
        _context.AppointmentReminders.RemoveRange(appointment.Reminders);
        appointment.Reminders.Clear();

        if (dto.Reminders != null && dto.Reminders.Any())
        {
            foreach (var r in dto.Reminders)
            {
                appointment.Reminders.Add(new AppointmentReminder
                {
                    MinutesBefore = r.MinutesBefore,
                    ScheduledTime = appointment.StartDate.AddMinutes(-r.MinutesBefore)
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(appointment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId);

        if (appointment == null) return NotFound();

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("due-reminders")]
    public async Task<IActionResult> GetDueReminders()
    {
        var now = DateTime.UtcNow;
        var reminders = await _context.AppointmentReminders
            .Include(r => r.Appointment)
            .Where(r => r.Appointment.UserId == UserId && !r.IsSent && r.ScheduledTime <= now)
            .ToListAsync();

        if (!reminders.Any()) return Ok(new List<object>());

        var result = reminders.Select(r => new
        {
            r.Id,
            r.AppointmentId,
            AppointmentTitle = r.Appointment.Title,
            r.MinutesBefore,
            r.ScheduledTime
        }).ToList();

        // Mark as sent
        foreach (var r in reminders)
        {
            r.IsSent = true;
        }
        await _context.SaveChangesAsync();

        return Ok(result);
    }
}

public class AppointmentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }

    public List<ReminderDto>? Reminders { get; set; }
}

public class ReminderDto
{
    public int MinutesBefore { get; set; }
}
