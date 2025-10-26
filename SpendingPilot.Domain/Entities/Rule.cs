namespace SpendingPilot.Domain.Entities;

public enum RulePatternType
{
    Contains = 0,
    Regex = 1,
    Merchant = 2,
    AmountRange = 3
}

public class Rule
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public RulePatternType PatternType { get; set; }
    public string? Pattern { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int CategoryId { get; set; }
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;

    public Category? Category { get; set; }
}
