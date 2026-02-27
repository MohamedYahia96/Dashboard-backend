using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<CourseTrack> CourseTracks => Set<CourseTrack>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<StickyNote> StickyNotes => Set<StickyNote>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentReminder> AppointmentReminders => Set<AppointmentReminder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TaskTag many-to-many
        builder.Entity<TaskTag>()
            .HasKey(tt => new { tt.TaskItemId, tt.TagId });

        builder.Entity<TaskTag>()
            .HasOne(tt => tt.TaskItem)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(tt => tt.TaskItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TaskTag>()
            .HasOne(tt => tt.Tag)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing for recurring tasks
        builder.Entity<TaskItem>()
            .HasOne(t => t.ParentTask)
            .WithMany()
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        // TaskItem → User
        builder.Entity<TaskItem>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskItem → Category
        builder.Entity<TaskItem>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tag → User
        builder.Entity<Tag>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Notification → User
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ActivityLog → User
        builder.Entity<ActivityLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attachment → TaskItem
        builder.Entity<Attachment>()
            .HasOne(a => a.TaskItem)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TaskItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.Entity<TaskItem>()
            .HasIndex(t => t.UserId);

        builder.Entity<TaskItem>()
            .HasIndex(t => t.CategoryId);

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        builder.Entity<ActivityLog>()
            .HasIndex(a => a.UserId);

        // CourseTrack → User
        builder.Entity<CourseTrack>()
            .HasOne(ct => ct.User)
            .WithMany()
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course → CourseTrack (cascade: deleting a track deletes its courses)
        builder.Entity<Course>()
            .HasOne(c => c.CourseTrack)
            .WithMany(ct => ct.Courses)
            .HasForeignKey(c => c.CourseTrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course → User
        builder.Entity<Course>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for courses
        builder.Entity<CourseTrack>()
            .HasIndex(ct => ct.UserId);

        builder.Entity<Course>()
            .HasIndex(c => new { c.UserId, c.CourseTrackId });

        // StickyNote → User
        builder.Entity<StickyNote>()
            .HasOne(sn => sn.User)
            .WithMany()
            .HasForeignKey(sn => sn.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StickyNote>()
            .HasIndex(sn => sn.UserId);

        // Appointment → User
        builder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // AppointmentReminder → Appointment
        builder.Entity<AppointmentReminder>()
            .HasOne(ar => ar.Appointment)
            .WithMany(a => a.Reminders)
            .HasForeignKey(ar => ar.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for Appointments
        builder.Entity<Appointment>()
            .HasIndex(a => a.UserId);
        
        builder.Entity<AppointmentReminder>()
            .HasIndex(ar => ar.IsSent);
    }
}
