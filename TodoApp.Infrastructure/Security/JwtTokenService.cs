// TodoApp.Infrastructure.Security

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        // ✅ Claims: artık Role da ekliyoruz
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.UserName ?? string.Empty),

            // ✅ Multi-Tenancy İzolasyonu İçin Kritik Satır
           new Claim("tenantId", user.TenantId.ToString()),

            // ✅ KRİTİK: Role-Based Authorization için
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
}
