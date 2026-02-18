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

    public async Task<PaginatedResult<TodoItem>> GetUserTodosAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken ct)
    {
        // 1. Temel sorguyu hazırla (Global Query Filter sayesinde IsDeleted=0 otomatik uygulanır)
        var query = _context.TodoItems.Where(t => t.UserId == userId);

        // 2. Arama Filtresi (Eğer arama kutusuna bir şey yazılmışsa)
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Title indeksi burada performans sağlar
            query = query.Where(t => t.Title.Contains(search));
        }

        // 3. Filtrelenmiş toplam kayıt sayısını al (Sayfalama için şart)
        var totalCount = await query.CountAsync(ct);

        // 4. Sıralama, Atlatma (Skip) ve Alma (Take) işlemlerini uygula
        var items = await query
            .OrderByDescending(t => t.CreatedAt) // En yeni en üstte
            .Skip((pageNumber - 1) * pageSize)   // Önceki sayfaları geç
            .Take(pageSize)                      // Sadece istenen sayfa kadarını getir
            .ToListAsync(ct);

        // 5. Sonucu profesyonel PaginatedResult zarfı içinde dön
        return new PaginatedResult<TodoItem>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
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
        // Soft Delete: Veriyi fiziksel olarak silmiyoruz
        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;

        _context.TodoItems.Update(todo);
        await _context.SaveChangesAsync(ct);
    }
}