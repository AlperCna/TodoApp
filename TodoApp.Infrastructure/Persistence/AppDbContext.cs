using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();

            b.Property(u => u.Email)
             .IsRequired()
             .HasMaxLength(256);

            b.Property(u => u.UserName)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.PasswordSalt).IsRequired();
        });

        // TODO ITEMS
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.Property(t => t.Title)
             .IsRequired()
             .HasMaxLength(200);

            // Indexes
            b.HasIndex(t => t.UserId);
            b.HasIndex(t => new { t.UserId, t.IsCompleted });

            // Relationships
            b.HasOne(t => t.User)
             .WithMany(u => u.TodoItems)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}   