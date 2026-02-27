namespace TaskDashboard.Api.Models;

public class AppointmentReminder
{
    public int Id { get; set; }
    public int MinutesBefore { get; set; } // e.g., 15 mins before
    public bool IsSent { get; set; } = false;

    // Display / Processing Optimization
    public DateTime ScheduledTime { get; set; } 

    // Relations
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
}
