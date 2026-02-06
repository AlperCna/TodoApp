using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

// Kullanıcı giriş yaparken gönderilen veriler
public record LoginRequest(string Email, string Password);