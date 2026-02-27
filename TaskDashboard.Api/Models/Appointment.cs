namespace TaskDashboard.Api.Models;

public class Appointment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Color { get; set; } = "#3b82f6"; // Default blue
    public bool IsAllDay { get; set; } = false;

    // Relations
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    
    public ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
}
