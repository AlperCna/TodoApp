using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Common;
using TodoApp.Application.Interfaces.Common;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Tüm ayarları bu assembly (proje) içinden otomatik yükler
        // modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Veya daha kontrollü olması için manuel ekleme:
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration(_currentTenantService));
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration(_currentTenantService));
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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