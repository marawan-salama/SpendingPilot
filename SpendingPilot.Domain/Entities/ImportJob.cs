namespace SpendingPilot.Domain.Entities;

public class ImportJob
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public int RowCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorsJson { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Pending";
}
