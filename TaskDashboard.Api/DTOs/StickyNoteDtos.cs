using System.ComponentModel.DataAnnotations;

namespace TaskDashboard.Api.DTOs;

public class StickyNoteDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Color { get; set; } = "#fff9c4";
    public double PosX { get; set; }
    public double PosY { get; set; }
    public int ZIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StickyNoteCreateDto
{
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Color { get; set; } = "#fff9c4";
    public double PosX { get; set; } = 100;
    public double PosY { get; set; } = 100;
    public int ZIndex { get; set; } = 1;
}

public class StickyNoteUpdateDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Color { get; set; }
    public double? PosX { get; set; }
    public double? PosY { get; set; }
    public int? ZIndex { get; set; }
}
