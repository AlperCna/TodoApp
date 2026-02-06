using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface ITodoRepository
{
    // Belirli bir kullanıcıya ait tüm görevleri getirir
    Task<IEnumerable<TodoItem>> GetUserTodosAsync(Guid userId, CancellationToken ct = default);

    // ID'ye göre tek bir görev getirir
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Yeni görev ekler
    Task AddAsync(TodoItem todo, CancellationToken ct = default);

    // Mevcut görevi günceller
    Task UpdateAsync(TodoItem todo, CancellationToken ct = default);

    // Görevi siler
    Task DeleteAsync(TodoItem todo, CancellationToken ct = default);
}