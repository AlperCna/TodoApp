using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class TodoItem : BaseEntity, ITenantEntity //  ITenantEntity eklendi
{
    public Guid TenantId { get; set; } //  Bu görev hangi kiracının?
    public Guid UserId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; } // Tenant bağlantısı
    public User? User { get; set; }
}