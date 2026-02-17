using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Common;

namespace TodoApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. USERS TABLOSU YAPILANDIRMASI
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();

            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.PasswordSalt).IsRequired();

            // MANUEL MIGRATION: Yeni eklenen Address alanı yapılandırması
            b.Property(u => u.Address).HasMaxLength(500);
        });

        // 2. TODOITEMS TABLOSU YAPILANDIRMASI
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.Property(t => t.Title).IsRequired().HasMaxLength(200);

            // SOFT DELETE: Global Query Filter
            // Bu satır sayesinde uygulama genelinde 'IsDeleted' olanlar asla gelmez.
            b.HasQueryFilter(t => !t.IsDeleted);

            // İndeksler: Sorgu performansını artırır
            b.HasIndex(t => t.UserId);
            b.HasIndex(t => new { t.UserId, t.IsCompleted });

            // İlişki: 1 User -> Many TodoItems
            b.HasOne(t => t.User)
             .WithMany(u => u.TodoItems)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // 3. OTOMATİK TARİH GÜNCELLEME (Audit Logging)
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}