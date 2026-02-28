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
    private readonly ITenantRepository _tenants;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(
        IUserRepository users,
        ITenantRepository tenants,
        IPasswordHasher hasher,
        IJwtTokenService jwt)
    {
        _users = users;
        _tenants = tenants;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();
        var tenantName = request.TenantName.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(tenantName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Geçersiz kayıt isteği. Tüm alanları doldurun.");
        }

        var tenant = await _tenants.GetByNameAsync(tenantName, ct);
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = tenantName,
                CreatedAt = DateTime.UtcNow
            };
            await _tenants.AddAsync(tenant, ct);
        }

        if (await _users.EmailExistsAsync(email, ct))
            throw new InvalidOperationException("Email zaten kayıtlı.");

        var hash = _hasher.HashPassword(request.Password, out var salt);

        // Refresh Token Üretimi
        var refreshToken = _jwt.GenerateRefreshToken();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            TenantId = tenant.Id,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            // ✅ Veritabanına yeni kolonları yazıyoruz
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7) // 7 Günlük yedek anahtar
        };

        await _users.AddAsync(user, ct);

        var token = _jwt.CreateToken(user);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token,
            RefreshToken: refreshToken // Response'a eklendi
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        var ok = _hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // ✅ Login anında hem Access hem Refresh Token yenilenir
        var token = _jwt.CreateToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        // ✅ DB Güncelleme
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _users.UpdateAsync(user, ct);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token,
            RefreshToken: refreshToken
        );
    }

    // Yeni: Token Yenileme Mantığı
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        // Kullanıcıyı Refresh Token üzerinden buluyoruz
        var user = await _users.GetByRefreshTokenAsync(request.RefreshToken, ct);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Oturum süresi dolmuş veya geçersiz anahtar.");
        }

        // Token Rotation: Her yenilemede yeni bir Refresh Token veriyoruz (Güvenlik için)
        var newAccessToken = _jwt.CreateToken(user);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _users.UpdateAsync(user, ct);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: newAccessToken,
            RefreshToken: newRefreshToken
        );
    }
}