using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

public record RegisterRequest(string UserName, string Email, string Password);
