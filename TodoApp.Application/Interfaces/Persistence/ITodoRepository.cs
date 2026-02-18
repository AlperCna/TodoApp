using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs.Common;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface ITodoRepository
{
    // Search parametresi eklendi. Varsayılan olarak null (boş) atanabilir.
    Task<PaginatedResult<TodoItem>> GetUserTodosAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken ct = default);

    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(TodoItem todo, CancellationToken ct = default);

    Task UpdateAsync(TodoItem todo, CancellationToken ct = default);

    Task DeleteAsync(TodoItem todo, CancellationToken ct = default);
}