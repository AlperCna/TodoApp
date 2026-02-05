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

    // Navigation
    public User? User { get; set; }
}
