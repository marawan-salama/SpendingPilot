namespace SpendingPilot.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public int AccountId { get; set; }
    public DateTime PostedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public bool IsAutoCategorized { get; set; }

    public Account? Account { get; set; }
    public Category? Category { get; set; }
}
