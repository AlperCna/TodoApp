using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Common;
using TodoApp.Application.Interfaces.Common; // Service arayüzü için

namespace TodoApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    // 1. CONSTRUCTOR: ICurrentTenantService enjekte edildi
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService currentTenantService) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Tenant> Tenants => Set<Tenant>(); // Yeni Tenant tablosu

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 2. TENANT TABLOSU YAPILANDIRMASI
        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Name).IsRequired().HasMaxLength(200);
        });

        // 3. USERS TABLOSU YAPILANDIRMASI
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.PasswordSalt).IsRequired();
            b.Property(u => u.Address).HasMaxLength(500);
            b.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("User");

            // MULTI-TENANCY FİLTRESİ: Kullanıcı sadece kendi şirketindekileri görür
            b.HasQueryFilter(u => u.TenantId == _currentTenantService.TenantId);
        });

        // 4. TODOITEMS TABLOSU YAPILANDIRMASI
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.Property(t => t.Title).IsRequired().HasMaxLength(200);

            // BİRLEŞTİRİLMİŞ GLOBAL QUERY FILTER (Soft Delete + Multi-Tenancy)
            // Önemli: EF Core tek bir filtre kabul ettiği için ikisini '&&' ile bağladık.
            b.HasQueryFilter(t => !t.IsDeleted && t.TenantId == _currentTenantService.TenantId);

            // PERFORMANS İNDEKSLERİ (TenantId eklendi - Sorgular bu sütun üzerinden yürüyecek)
            b.HasIndex(t => new { t.TenantId, t.UserId, t.IsDeleted })
             .HasDatabaseName("IX_TodoItems_Tenant_User_Deleted");

            b.HasIndex(t => t.Title).HasDatabaseName("IX_TodoItems_Title");

            b.HasOne(t => t.User)
             .WithMany(u => u.TodoItems)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Tenant ilişkisi
            b.HasOne(t => t.Tenant)
             .WithMany(ten => ten.TodoItems)
             .HasForeignKey(t => t.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // 5. OTOMATİK TARİH VE TENANT ID GÜNCELLEME
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            // Tarih Güncellemeleri (BaseEntity)
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                    baseEntity.CreatedAt = DateTime.UtcNow;
                else if (entry.State == EntityState.Modified)
                    baseEntity.UpdatedAt = DateTime.UtcNow;
            }

            // OTOMATİK TENANT ID ATAMA (ITenantEntity)
            if (entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                // Eğer manuel atanmadıysa, servisten gelen ID'yi bas
                if (tenantEntity.TenantId == Guid.Empty)
                {
                    tenantEntity.TenantId = _currentTenantService.TenantId ?? Guid.Empty;
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}