using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;

    // Güvenlik: şifreyi plaintext tutmuyoruz
    public string PasswordHash { get; set; } = default!;
    public string PasswordSalt { get; set; } = default!;

    public string? Address { get; set; }

    public string Role { get; set; } = "User";


    // Navigation
    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
