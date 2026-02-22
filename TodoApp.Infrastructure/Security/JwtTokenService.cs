using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography; // ✅ Kriptografik rastgelelik için eklendi
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Application.Interfaces.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    // --- 1. ACCESS TOKEN ÜRETİMİ (JWT) ---
    public string CreateToken(User user)
    {
        var secret = _config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret missing.");

        var issuer = _config["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer missing.");

        var audience = _config["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience missing.");

        var expiryStr = _config["JwtSettings:ExpiryMinutes"]
            ?? throw new InvalidOperationException("JwtSettings:ExpiryMinutes missing.");

        if (!double.TryParse(expiryStr, out var expiryMinutes))
            throw new InvalidOperationException("JwtSettings:ExpiryMinutes invalid.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ✅ Claims: Multi-tenancy ve Role desteği korunuyor
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.UserName ?? string.Empty),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // --- 2. REFRESH TOKEN ÜRETİMİ (Rastgele Anahtar) ---
    // Bu metod veritabanındaki yeni RefreshToken kolonunu besleyecek
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}