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
    private readonly ITenantRepository _tenants; // ✅ 3. Adım: Yeni eklendi
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public AuthService(
        IUserRepository users,
        ITenantRepository tenants, // ✅ Dependency Injection'a eklendi
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

        // Normalize
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();
        var tenantName = request.TenantName.Trim(); // ✅ Yeni: Şirket adını yakala

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(tenantName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Geçersiz kayıt isteği. Tüm alanları doldurun.");
        }

        // 1️⃣ Dinamik Şirket (Tenant) Yönetimi
        var tenant = await _tenants.GetByNameAsync(tenantName, ct);

        // Eğer şirket yoksa, yeni bir tane oluştur (SaaS mantığı)
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

        // Email kontrolü (IgnoreQueryFilters ile tüm sistemde kontrol eder)
        if (await _users.EmailExistsAsync(email, ct))
            throw new InvalidOperationException("Email zaten kayıtlı.");

        // Hash + salt
        var hash = _hasher.HashPassword(request.Password, out var salt);

        // 2️⃣ Kullanıcıyı ilgili TenantId ile oluştur
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            TenantId = tenant.Id, // 👈 KRİTİK: Kullanıcı artık sahipsiz değil!
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User", // Dilersen ilk kullanıcıyı Admin yapabilirsin
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Persist
        await _users.AddAsync(user, ct);

        // Token üretirken artık içindeki TenantId bilgisi de JwtTokenService'e gidecek
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

        // User fetch (UserRepository içindeki IgnoreQueryFilters sayesinde TenantId bilmeden çekeriz)
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // Verify password
        var ok = _hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!ok)
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");

        // Token üretimi (User içindeki TenantId otomatik olarak Jwt'ye eklenecek)
        var token = _jwt.CreateToken(user);

        return new AuthResponse(
            Id: user.Id,
            UserName: user.UserName,
            Email: user.Email,
            Token: token
        );
    }
}