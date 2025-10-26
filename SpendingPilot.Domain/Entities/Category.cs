namespace SpendingPilot.Domain.Entities;

public enum CategoryType
{
    Expense = 0,
    Income = 1
}

public class Category
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public CategoryType Type { get; set; }
}
