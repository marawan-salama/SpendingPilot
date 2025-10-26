using System.Globalization;
namespace SpendingPilot.Infrastructure.Importing;

public record ImportResult(int Imported, int Skipped, int Errors);

public interface ITransactionImportService
{
    Task<ImportResult> ImportCsvAsync(string userId, Stream csvStream, string? accountFallbackName = null, CancellationToken ct = default);
}
