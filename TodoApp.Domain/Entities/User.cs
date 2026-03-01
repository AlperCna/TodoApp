using System;
using System.Collections.Generic;
using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class User : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; } // Hangi kiracıya ait?
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;

    // Güvenlik: SSO kullanıcılarında buralar boş kalabilir
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }

    // Yeni Eklenen SSO Alanları
    public string? ExternalProvider { get; set; } // 'Microsoft' veya 'Google'
    public string? ExternalId { get; set; }       // Dış servisin verdiği benzersiz ID

    public string? Address { get; set; }
    public string Role { get; set; } = "User";

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}