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
        // ✅ KRİTİK: Giriş anında filtreyi devre dışı bırakıyoruz
        // Çünkü giriş yaparken sistem henüz TenantId'yi bilmiyor.
        return await _context.Users
            .IgnoreQueryFilters() // 👈 Bu metot filtreyi bu sorgu için kapatır
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        // Kayıt olurken de email'in sistemde (tüm tenantlar dahil) olup olmadığını kontrol etmeliyiz
        return await _context.Users
            .IgnoreQueryFilters() // 👈 Email kontrolü global (tüm sistemde) olmalı
            .AnyAsync(u => u.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }
}