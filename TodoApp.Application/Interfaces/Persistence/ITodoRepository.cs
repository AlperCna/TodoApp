using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs.Common; // PaginatedResult burada
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Interfaces.Persistence;

public interface ITodoRepository
{
    // Sayfalama için geri dönüş tipi PaginatedResult olarak güncellendi
    // pageNumber ve pageSize parametreleri eklendi
    Task<PaginatedResult<TodoItem>> GetUserTodosAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(TodoItem todo, CancellationToken ct = default);

    Task UpdateAsync(TodoItem todo, CancellationToken ct = default);

    Task DeleteAsync(TodoItem todo, CancellationToken ct = default);
}