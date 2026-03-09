using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Domain.Common;
using TodoApp.Domain.Entities;
// 🛡️ BURAYI DÜZELTTİK: Klasör adın 'i' ile bittiği için burası da öyle olmalı
using TodoApp.Infrastructure.Persistence.Configirations;
using TodoApp.Infrastructure.Persistence.Configurations;

namespace TodoApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService currentTenantService) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Konfigürasyonların Kaydedilmesi ---
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration(_currentTenantService));

        // 🛡️ BURAYI DÜZELTTİK: Dosya ismin 'i' ile olduğu için sınıf ismin de muhtemelen öyledir
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration(_currentTenantService));

        modelBuilder.ApplyConfiguration(new UserRefreshTokenConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ... (Kalan SaveChanges kodların aynı kalsın)
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                    baseEntity.CreatedAt = DateTime.UtcNow;
                else if (entry.State == EntityState.Modified)
                    baseEntity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.TenantId == Guid.Empty)
                {
                    tenantEntity.TenantId = _currentTenantService.TenantId ?? Guid.Empty;
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}