using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SpendingPilot.Domain.Entities;
using SpendingPilot.Infrastructure.Rules;

namespace SpendingPilot.Infrastructure.Importing;

public class TransactionImportService : ITransactionImportService
{
    private readonly AppDbContext _db;
    private readonly IRuleEngine _rules;

    public TransactionImportService(AppDbContext db, IRuleEngine rules)
    {
        _db = db;
        _rules = rules;
    }

    public async Task<ImportResult> ImportCsvAsync(string userId, Stream csvStream, string? accountFallbackName = null, CancellationToken ct = default)
    {
        var job = new ImportJob
        {
            UserId = userId,
            FileName = "upload.csv",
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };
        _db.ImportJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        var imported = 0; var skipped = 0; var errors = 0; var errorList = new List<string>();

        using var reader = new StreamReader(csvStream);
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            DetectDelimiter = true
        };
        using var csv = new CsvReader(reader, cfg);
        await foreach (var row in csv.GetRecordsAsync<dynamic>(ct))
        {
            try
            {
                var dict = (IDictionary<string, object>)row;
                string Get(string key)
                {
                    foreach (var k in dict.Keys)
                    {
                        if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                            return Convert.ToString(dict[k]) ?? string.Empty;
                    }
                    return string.Empty;
                }

                var dateStr = Get("Date");
                var desc = Get("Description");
                var merch = Get("Merchant");
                var amountStr = Get("Amount");
                var accountName = Get("Account");
                var categoryName = Get("Category");

                if (string.IsNullOrWhiteSpace(accountName))
                    accountName = accountFallbackName ?? "Checking";

                if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    skipped++; continue;
                }
                if (!decimal.TryParse(amountStr, NumberStyles.Number | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount))
                {
                    skipped++; continue;
                }

                var account = await _db.Accounts.FirstOrDefaultAsync(a => a.UserId == userId && a.Name == accountName, ct);
                if (account is null)
                {
                    account = new Account { UserId = userId, Name = accountName };
                    _db.Accounts.Add(account);
                    await _db.SaveChangesAsync(ct);
                }

                int? categoryId = null;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    var cat = await _db.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name == categoryName, ct);
                    if (cat is null)
                    {
                        cat = new Category { UserId = userId, Name = categoryName, Type = amount < 0 ? CategoryType.Expense : CategoryType.Income };
                        _db.Categories.Add(cat);
                        await _db.SaveChangesAsync(ct);
                    }
                    categoryId = cat.Id;
                }

                var tx = new Domain.Entities.Transaction
                {
                    UserId = userId,
                    AccountId = account.Id,
                    PostedAt = date,
                    Description = desc ?? string.Empty,
                    Merchant = string.IsNullOrWhiteSpace(merch) ? null : merch,
                    Amount = amount,
                    CategoryId = categoryId,
                    IsAutoCategorized = false
                };

                if (categoryId is null)
                {
                    var assignedId = await _rules.TryCategorizeAsync(userId, tx, ct);
                    if (assignedId is int cid)
                    {
                        tx.CategoryId = cid;
                        tx.IsAutoCategorized = true;
                    }
                }

                _db.Transactions.Add(tx);
                imported++;
            }
            catch (Exception ex)
            {
                errors++;
                errorList.Add(ex.Message);
            }
        }

        job.RowCount = imported + skipped;
        job.ErrorCount = errors;
        job.ErrorsJson = errorList.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(errorList);
        job.Status = "Completed";
        job.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new ImportResult(imported, skipped, errors);
    }
}
