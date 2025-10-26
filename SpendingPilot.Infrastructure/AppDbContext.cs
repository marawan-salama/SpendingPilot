using Microsoft.EntityFrameworkCore;
using SpendingPilot.Domain.Entities;

namespace SpendingPilot.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Account>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.UserId).IsRequired();
            e.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
        });

        b.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.UserId).IsRequired();
            e.HasIndex(x => new { x.UserId, x.Name, x.Type }).IsUnique();
        });

        b.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.UserId).IsRequired();
            e.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.UserId, x.PostedAt });
        });

        b.Entity<Rule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Pattern).HasMaxLength(300);
            e.Property(x => x.UserId).IsRequired();
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.Priority });
        });

        b.Entity<ImportJob>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(260);
            e.Property(x => x.UserId).IsRequired();
            e.HasIndex(x => new { x.UserId, x.StartedAt });
        });
    }
}
