using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, string provider, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.ExternalProvider == provider, ct);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // --- YENİ REFRESH TOKEN TABLO METODLARI ---

    public async Task AddRefreshTokenAsync(UserRefreshToken token, CancellationToken ct = default)
    {
        await _context.UserRefreshTokens.AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<UserRefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        // ÖNEMLİ: Token kontrolü yaparken .Include(u => u.User) kullanarak 
        // kullanıcı bilgilerini de tek seferde çekiyoruz (Eager Loading).
        return await _context.UserRefreshTokens
            .IgnoreQueryFilters() // Güvenlik kontrolü olduğu için tenant filtresini geçiyoruz
            .Include(u => u.User)
            .FirstOrDefaultAsync(t => t.Token == token, ct);
    }

    public async Task UpdateRefreshTokenAsync(UserRefreshToken token, CancellationToken ct = default)
    {
        _context.UserRefreshTokens.Update(token);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        // Kullanıcının sistemdeki tüm aktif/geçerli tokenlarını bul ve iptal et
        var activeTokens = await _context.UserRefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}