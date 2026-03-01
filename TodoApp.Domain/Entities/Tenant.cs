using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = default!; // Şirket Adı

    // Yeni Eklenen: SSO domain eşleşmesi için (Örn: fsm.edu.tr)
    public string? Domain { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}