using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Application.DTOs.Auth;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Interfaces.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        // Normalize
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Geçersiz kayıt isteği.");
        }

        // Email unique
        if (await _users.EmailExistsAsync(email, ct))
            throw new InvalidOperationException("Email zaten kayıtlı.");

        // Hash + salt
        var hash = _hasher.HashPassword(request.Password, out var salt);

        // User create
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Persist
        await _users.AddAsync(user, ct);

        // Token
        var token = _jwt.CreateToken(user);

        // Response
        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        // Normalize
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // User fetch
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // Verify password
        var ok = _hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // Token
        var token = _jwt.CreateToken(user);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token
        );
    }
}
