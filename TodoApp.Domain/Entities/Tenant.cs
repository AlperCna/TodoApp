using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = default!; // Şirket Adı

    // Navigation: Bir tenant'ın birden fazla kullanıcısı ve görevi olabilir
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}