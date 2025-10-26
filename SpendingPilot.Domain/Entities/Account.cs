namespace SpendingPilot.Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Institution { get; set; }
    public string? Type { get; set; }
}
