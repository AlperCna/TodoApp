using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

// TodoApp.Application.DTOs.Auth/RegisterRequest.cs
// TenantName parametresini ekliyoruz.
public record RegisterRequest(string UserName, string Email, string Password, string TenantName);