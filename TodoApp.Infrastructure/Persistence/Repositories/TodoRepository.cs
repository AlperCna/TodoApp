using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.DTOs.Common;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Hocanın istediği merkezi Paginated metot
    public async Task<PaginatedResult<TodoItem>> GetTodosAsync(
        Guid? userId,
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken ct)
    {
        // 1. Temel sorguyu hazırla (AsQueryable ile sorguyu belleğe çekmeden oluşturuyoruz)
        // Global Query Filter (Soft Delete + Tenant) zaten Context seviyesinde otomatik uygulanıyor.
        var query = _context.TodoItems.AsQueryable();

        // 2. Dinamik Filtreleme: 
        // Eğer bir userId gönderilmişse (Normal kullanıcı ise) sadece onun görevlerini filtrele.
        // Eğer null gönderilmişse (Admin ise) Where koşulunu ekleme, tüm todoları kapsa.
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        // 3. Arama Filtresi (Hem Admin hem User için ortak çalışır)
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Title üzerindeki indeks burada performans sağlar
            query = query.Where(t => t.Title.Contains(search));
        }

        // 4. Filtrelenmiş toplam kayıt sayısını al (Sayfalama bileşeni için şart)
        var totalCount = await query.CountAsync(ct);

        // 5. Sayfalama işlemlerini (Skip/Take) uygula ve veritabanına sorguyu fırlat
        var items = await query
            .OrderByDescending(t => t.CreatedAt) // En yeni kayıtlar her zaman üstte
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // 6. Sonucu PaginatedResult zarfı içinde profesyonelce dön
        return new PaginatedResult<TodoItem>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // FindAsync, Primary Key üzerinden hızlı arama yapar
        return await _context.TodoItems.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(TodoItem todo, CancellationToken ct)
    {
        await _context.TodoItems.AddAsync(todo, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TodoItem todo, CancellationToken ct)
    {
        _context.TodoItems.Update(todo);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TodoItem todo, CancellationToken ct)
    {
        // 🗑️ Soft Delete Uygulaması
        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;

        _context.TodoItems.Update(todo);
        await _context.SaveChangesAsync(ct);
    }
}