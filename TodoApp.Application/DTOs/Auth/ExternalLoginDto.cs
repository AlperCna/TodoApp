using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

public class ExternalLoginDto
{
    public string Email { get; set; } = default!;
    public string ExternalId { get; set; } = default!;
    public string Provider { get; set; } = default!; // Microsoft veya Google
    public string Domain { get; set; } = default!; // fsm.edu.tr gibi
}