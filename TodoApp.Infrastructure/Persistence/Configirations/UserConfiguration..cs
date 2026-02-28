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

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private readonly ICurrentTenantService _currentTenantService;

    public UserConfiguration(ICurrentTenantService currentTenantService)
    {
        _currentTenantService = currentTenantService;
    }

    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.Email).IsRequired().HasMaxLength(256);
        b.Property(u => u.UserName).IsRequired().HasMaxLength(100);
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.PasswordSalt).IsRequired();
        b.Property(u => u.Address).HasMaxLength(500);
        b.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("User");

        // MULTI-TENANCY FİLTRESİ
        b.HasQueryFilter(u => u.TenantId == _currentTenantService.TenantId);
    }
}