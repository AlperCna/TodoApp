using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Text;
using TodoApp.Application.Interfaces.Security;

namespace TodoApp.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password, out string salt)
    {
        using var hmac = new HMACSHA512();
        salt = Convert.ToBase64String(hmac.Key); // Anahtarı metne çevirip saklıyoruz

        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var hmac = new HMACSHA512(saltBytes);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == hash;
    }
}