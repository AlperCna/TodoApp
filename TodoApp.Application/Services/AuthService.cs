using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor; // IP takibi için eklendi

    public AuthService(
        IUserRepository users,
        ITenantRepository tenants,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IHttpContextAccessor httpContextAccessor)
    {
        _users = users;
        _tenants = tenants;
        _hasher = hasher;
        _jwt = jwt;
        _httpContextAccessor = httpContextAccessor;
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

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            TenantId = tenant.Id,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);

        // ✅ Yeni Tablo Mantığı: Refresh Token Üretimi ve Kaydı
        var refreshTokenStr = _jwt.GenerateRefreshToken();
        var userRefreshToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenStr,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            IsRevoked = false
        };

        await _users.AddRefreshTokenAsync(userRefreshToken, ct);

        var token = _jwt.CreateToken(user);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token,
            RefreshToken: refreshTokenStr // Response'a eklendi
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

        var ok = _hasher.VerifyPassword(request.Password, user.PasswordHash!, user.PasswordSalt!);
        if (!ok)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // ✅ Login anında hem Access hem Refresh Token yenilenir
        var token = _jwt.CreateToken(user);
        var refreshTokenStr = _jwt.GenerateRefreshToken();

        // ✅ Yeni Tabloya Kayıt (Eskileri silmiyoruz, geçmiş tutuyoruz)
        var userRefreshToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenStr,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        };

        await _users.AddRefreshTokenAsync(userRefreshToken, ct);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token,
            RefreshToken: refreshTokenStr
        );
    }

    // Yeni: Token Yenileme Mantığı (Ayrı Tabloya Göre)
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        // ✅ Yeni: Refresh Token'ı kendi tablosundan, User ile birlikte buluyoruz
        var storedToken = await _users.GetRefreshTokenAsync(request.RefreshToken, ct);

        if (storedToken == null || !storedToken.IsActive)
        {
            throw new UnauthorizedAccessException("Oturum süresi dolmuş veya geçersiz anahtar.");
        }

        // ✅ Token Rotation: Eski tokenı iptal et
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _users.UpdateRefreshTokenAsync(storedToken, ct);

        // Yeni tokenları üret
        var user = storedToken.User;
        var newAccessToken = _jwt.CreateToken(user);
        var newRefreshTokenStr = _jwt.GenerateRefreshToken();

        // ✅ Yeni tokenı tabloya kaydet
        var newUserRefreshToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = newRefreshTokenStr,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        };

        await _users.AddRefreshTokenAsync(newUserRefreshToken, ct);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: newAccessToken,
            RefreshToken: newRefreshTokenStr
        );
    }

    public async Task<AuthResponse> HandleExternalLoginAsync(ExternalLoginDto request, CancellationToken ct = default)
    {
        // 1. Kullanıcı zaten kayıtlı mı?
        var user = await _users.GetByExternalIdAsync(request.ExternalId, request.Provider, ct);

        if (user == null)
        {
            var tenant = await _tenants.GetByDomainAsync(request.Domain, ct);

            if (tenant == null)
            {
                throw new UnauthorizedAccessException($"'{request.Domain}' alan adı için kayıtlı bir şirket (Tenant) bulunamadı.");
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email.Split('@')[0],
                TenantId = tenant.Id,
                ExternalProvider = request.Provider,
                ExternalId = request.ExternalId,
                CreatedAt = DateTime.UtcNow,
                Role = "User",
                PasswordHash = "SSO_USER_" + Guid.NewGuid().ToString("N"),
                PasswordSalt = Guid.NewGuid().ToString("N")
            };

            await _users.AddAsync(user, ct);
        }

        // 3. Tokenları Üret
        var accessToken = _jwt.CreateToken(user);
        var refreshTokenStr = _jwt.GenerateRefreshToken();

        // ✅ 4. Yeni Tabloya Refresh Token Kaydı
        var userRefreshToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenStr,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()


        };

        await _users.AddRefreshTokenAsync(userRefreshToken, ct);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: accessToken,
            RefreshToken: refreshTokenStr
        );
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // ✅ Yeni: Tablodan tokenı bul
        var storedToken = await _users.GetRefreshTokenAsync(refreshToken, ct);

        if (storedToken != null)
        {
            // Revoke işlemi: Sadece bu tokenı "öldürüyoruz"
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _users.UpdateRefreshTokenAsync(storedToken, ct);
        }
    }
}