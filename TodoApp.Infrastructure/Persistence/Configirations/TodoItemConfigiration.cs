using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;
using TodoApp.Application.Interfaces.Common;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    private readonly ICurrentTenantService _currentTenantService;

    public TodoItemConfiguration(ICurrentTenantService currentTenantService)
    {
        _currentTenantService = currentTenantService;
    }

    public void Configure(EntityTypeBuilder<TodoItem> b)
    {
        b.Property(t => t.Title).IsRequired().HasMaxLength(200);

        // GLOBAL QUERY FILTER (Soft Delete + Multi-Tenancy)
        b.HasQueryFilter(t => !t.IsDeleted && t.TenantId == _currentTenantService.TenantId);

        b.HasIndex(t => new { t.TenantId, t.UserId, t.IsDeleted })
         .HasDatabaseName("IX_TodoItems_Tenant_User_Deleted");

        b.HasIndex(t => t.Title).HasDatabaseName("IX_TodoItems_Title");

        b.HasOne(t => t.User)
         .WithMany(u => u.TodoItems)
         .HasForeignKey(t => t.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(t => t.Tenant)
         .WithMany(ten => ten.TodoItems)
         .HasForeignKey(t => t.TenantId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}