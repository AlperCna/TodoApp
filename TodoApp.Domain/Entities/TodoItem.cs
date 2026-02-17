using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Domain.Common;

namespace TodoApp.Domain.Entities;

public class TodoItem : BaseEntity
{
    public Guid UserId { get; set; }           // ownership için kritik
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsDeleted { get; set; } = false; // Verinin silinme durumu
    public DateTime? DeletedAt { get; set; }     // Ne zaman silindiği bilgisi

    // Navigation
    public User? User { get; set; }
}
