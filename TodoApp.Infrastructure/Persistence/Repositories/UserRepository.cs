using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    // ✅ YENI: Refresh Token ile arama
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters() // 👈 Tenant kısıtlamasına takılmadan tüm sistemde ara
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    // ✅ YENI: Kullanıcıyı (Refresh Token alanlarını) güncelleme
    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }
}