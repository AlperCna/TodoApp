using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Auth;

// RefreshToken eklendi
public record AuthResponse(Guid Id, string UserName, string Email, string Token, string RefreshToken);

// Yenileme isteği için yeni bir DTO
public record RefreshTokenRequest(string RefreshToken);