using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

// Giriş başarılı olduktan sonra kullanıcıya dönülecek bilgiler
public record AuthResponse(Guid Id, string UserName, string Email, string Token);