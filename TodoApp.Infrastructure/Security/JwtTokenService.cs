using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Biletin içine kimlik bilgilerini (Claims) gömüyoruz
        var claims = new[]
{
    // JwtRegisteredClaimNames.Sub yerine ClaimTypes.NameIdentifier kullanıyoruz
    // Bu, .NET'in kullanıcı kimliği için kullandığı resmi anahtardır.
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim("username", user.UserName)
};

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}