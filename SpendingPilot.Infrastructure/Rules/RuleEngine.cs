using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SpendingPilot.Domain.Entities;

namespace SpendingPilot.Infrastructure.Rules;

public class RuleEngine : IRuleEngine
{
    private readonly AppDbContext _db;

    public RuleEngine(AppDbContext db) => _db = db;

    public async Task<int?> TryCategorizeAsync(string userId, Transaction tx, CancellationToken ct = default)
    {
        var rules = await _db.Rules
            .Include(r => r.Category)
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        foreach (var rule in rules)
        {
            if (rule.CategoryId == 0) continue;
            if (Matches(rule, tx)) return rule.CategoryId;
        }
        return null;
    }

    private static bool Matches(Rule rule, Transaction tx)
    {
        switch (rule.PatternType)
        {
            case RulePatternType.Contains:
                if (string.IsNullOrWhiteSpace(rule.Pattern)) return false;
                var text = (tx.Merchant ?? tx.Description ?? string.Empty);
                return text.IndexOf(rule.Pattern, StringComparison.OrdinalIgnoreCase) >= 0;

            case RulePatternType.Regex:
                if (string.IsNullOrWhiteSpace(rule.Pattern)) return false;
                return Regex.IsMatch(tx.Merchant ?? tx.Description ?? string.Empty, rule.Pattern, RegexOptions.IgnoreCase);

            case RulePatternType.Merchant:
                if (string.IsNullOrWhiteSpace(rule.Pattern)) return false;
                return string.Equals(tx.Merchant ?? string.Empty, rule.Pattern, StringComparison.OrdinalIgnoreCase);

            case RulePatternType.AmountRange:
                var min = rule.MinAmount ?? decimal.MinValue;
                var max = rule.MaxAmount ?? decimal.MaxValue;
                return tx.Amount >= min && tx.Amount <= max;

            default:
                return false;
        }
    }
}
