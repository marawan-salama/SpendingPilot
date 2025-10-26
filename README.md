# SpendingPilot

A secure ASP.NET Core MVC application to track and analyze personal spending. Users can upload bank CSVs, auto-categorize transactions via rules, visualize spending with charts, export data, and (next) get ML-based predictions of next month’s spending.

## Tech stack

- Frontend: ASP.NET Core MVC + Razor views, Bootstrap, Chart.js
- Backend: ASP.NET Core 9 (compatible with .NET 9 SDK)
- Auth: ASP.NET Core Identity (cookie auth)
- Data: Entity Framework Core 9 + SQLite (dev)
- File import: CsvHelper
- ML: ML.NET (planned: regression model for monthly spend prediction)
- Background tasks: IHostedService (planned: scheduled re-training)
- Tests: xUnit + FluentAssertions
- Tooling: dotnet CLI, EF Core migrations

## Solution structure

- `SpendingPilot.Web` — MVC app, Identity, DI wiring, controllers and views
- `SpendingPilot.Infrastructure` — `AppDbContext`, import service, rules engine
- `SpendingPilot.Domain` — Entities and enums (Account, Category, Rule, Transaction, ImportJob)
- `SpendingPilot.Tests` — Unit tests (placeholder)

## Features implemented

- Authentication with ASP.NET Core Identity
- Domain entities and EF Core with SQLite
- CSV import using CsvHelper
  - Per-user ownership
  - Auto-category via rules engine (Contains, Regex, Merchant, AmountRange)
  - Creates accounts/categories as needed
- Transactions UI
  - List latest transactions
  - Upload CSV page
- Categories management (CRUD)
- Rules management (CRUD)
- Dashboard with Chart.js
  - Spend by category (current month)
  - Monthly spend trend (last 6 months)
- Export transactions to CSV with optional date range

## Roadmap / next

- ML.NET regression model to predict next month’s spending
- Optional IHostedService to re-train monthly
- CI (GitHub Actions) for build/test/migrations validation

## Getting started

### Prerequisites

- .NET SDK 9.x
- macOS/Linux/Windows

### Setup

```bash
# From repo root
# Restore and build
$ dotnet build

# Apply database migrations (Identity + AppDbContext)
$ dotnet ef database update --project SpendingPilot.Web --startup-project SpendingPilot.Web --context SpendingPilot.Web.Data.ApplicationDbContext
$ dotnet ef database update --project SpendingPilot.Web --startup-project SpendingPilot.Web --context SpendingPilot.Infrastructure.AppDbContext

# Run the app
$ dotnet run --project SpendingPilot.Web
# App will launch on http://localhost:5140 (or as shown in console)
```

### Configuration

- Connection string: `SpendingPilot.Web/appsettings.json` (`DefaultConnection` SQLite file `app.db`)
- Dev HTTPS: template cert installed; you can trust via `dotnet dev-certs https --trust`

## Usage

1. Register and sign in
2. Go to Transactions → Upload. Upload a CSV with columns:
   - Required: `Date, Description, Amount`
   - Optional: `Merchant, Account, Category`
3. View Transactions list (latest 50)
4. Manage Categories and Rules
5. See Dashboard charts
6. Export CSV at `/Transactions/Export?from=yyyy-MM-dd&to=yyyy-MM-dd`

## Entities (concise)

- User: IdentityUser (per-user data via UserId on all entities)
- Account: Name, Institution, Type, UserId
- Category: Name, Type (Expense/Income), UserId
- Rule: PatternType (Contains/Regex/Merchant/AmountRange), Pattern/Min/Max, CategoryId, Priority, IsActive, UserId
- Transaction: AccountId, PostedAt, Description, Merchant, Amount, CategoryId, IsAutoCategorized, UserId
- ImportJob: FileName, Status, RowCount, ErrorsJson, StartedAt, CompletedAt, UserId

## CSV format

- Date format: `yyyy-MM-dd`
- Decimal separator: `.`; negative amounts = expenses
- Columns can be in any case; importer matches headers case-insensitively

## Testing

- Test project scaffolded: `SpendingPilot.Tests`
- Add tests for import service, rules engine, and controllers


