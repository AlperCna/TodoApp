using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace TodoApp.Application.DTOs;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}