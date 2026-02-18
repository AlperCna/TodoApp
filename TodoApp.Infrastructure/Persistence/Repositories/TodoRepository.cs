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

    // Sayfalama mantığı buraya eklendi
    public async Task<PaginatedResult<TodoItem>> GetUserTodosAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct)
    {
        // 1. Sorguyu hazırla (Filtreler ve sıralama)
        var query = _context.TodoItems
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        // 2. Toplam kayıt sayısını hesapla (Sayfalama için kritik)
        var totalCount = await query.CountAsync(ct);

        // 3. Skip ve Take ile verinin sadece ilgili parçasını çek
        var items = await query
            .Skip((pageNumber - 1) * pageSize) // Önceki sayfaları atla
            .Take(pageSize)                    // Sadece istenen sayfa kadar al
            .ToListAsync(ct);

        // 4. Sonucu PaginatedResult zarfı içinde döndür
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
        // Soft Delete: Veriyi silmiyoruz, işaretliyoruz
        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;

        _context.TodoItems.Update(todo);
        await _context.SaveChangesAsync(ct);
    }
}