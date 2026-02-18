using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs.Common; // PaginatedResult için
using TodoApp.Application.DTOs.Todo;

namespace TodoApp.Application.Services.Todo;

public interface ITodoService
{
    Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct = default);
    
    // HATA BURADAYDI: Dönüş tipini PaginatedResult yaptık ve parametreleri ekledik
    Task<PaginatedResult<TodoResponse>> GetMyTodosAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    
    Task<TodoResponse> GetByIdMineAsync(Guid id, CancellationToken ct = default);
    Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<TodoResponse> ToggleCompleteAsync(Guid id, CancellationToken ct = default);
}