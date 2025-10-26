using SpendingPilot.Domain.Entities;

namespace SpendingPilot.Infrastructure.Rules;

public interface IRuleEngine
{
    Task<int?> TryCategorizeAsync(string userId, Transaction tx, CancellationToken ct = default);
}
