using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
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

    // Sadece belirli bir kullanıcıya ait olan Todo'ları getirir
    public async Task<IEnumerable<TodoItem>> GetUserTodosAsync(Guid userId, CancellationToken ct)
    {
        return await _context.TodoItems
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
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
        // Veriyi gerçekten silmiyoruz (Hard Delete iptal!)
        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;

        // Veriyi sadece güncelliyoruz
        _context.TodoItems.Update(todo);
        await _context.SaveChangesAsync(ct);
    }
}