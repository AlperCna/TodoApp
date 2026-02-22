using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoApp.Application.DTOs.Common;
using TodoApp.Application.DTOs.Todo;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Domain.Entities;
using Ganss.Xss; // ✅ Güvenlik kütüphanesi eklendi

namespace TodoApp.Application.Services.Todo;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly HtmlSanitizer _sanitizer; // ✅ Merkezi temizleyici

    public TodoService(ITodoRepository todoRepository, ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _currentUserService = currentUserService;
        _sanitizer = new HtmlSanitizer(); // ✅ Sanitizer yapılandırması
    }

    // ARAMA VE SAYFALAMA BURADA BİRLEŞTİ
    public async Task<PaginatedResult<TodoResponse>> GetMyTodosAsync(int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        // 1. Repository'ye 'search' parametresini de gönderiyoruz
        var paginatedTodos = await _todoRepository.GetUserTodosAsync(userId, pageNumber, pageSize, search, ct);

        // 2. Ham TodoItem listesini TodoResponse listesine çeviriyoruz
        var mappedItems = paginatedTodos.Items.Select(MapToResponse).ToList();

        // 3. Sonucu PaginatedResult paketiyle geri dönüyoruz
        return new PaginatedResult<TodoResponse>(
            mappedItems,
            paginatedTodos.TotalCount,
            pageNumber,
            pageSize);
    }

    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        // 🧼 Veritabanına yazmadan önce "Yıkama" işlemi yapıyoruz
        var todo = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = _sanitizer.Sanitize(request.Title), // ✅ Script etiketlerini temizle
            Description = request.Description != null ? _sanitizer.Sanitize(request.Description) : null, // ✅ HTML içeriğini temizle
            DueDate = request.DueDate,
            UserId = userId,
            IsCompleted = false
        };

        await _todoRepository.AddAsync(todo, ct);
        return MapToResponse(todo);
    }

    public async Task<TodoResponse> GetByIdMineAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);
        if (todo == null || todo.UserId != userId) throw new UnauthorizedAccessException("Yetkisiz erişim.");
        return MapToResponse(todo);
    }

    public async Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);
        if (todo == null || todo.UserId != userId) throw new UnauthorizedAccessException("Güncelleme yetkiniz yok.");

        // 🧼 Güncelleme sırasında gelen verileri de sanitize ediyoruz
        todo.Title = _sanitizer.Sanitize(request.Title);
        todo.Description = request.Description != null ? _sanitizer.Sanitize(request.Description) : null;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate;

        await _todoRepository.UpdateAsync(todo, ct);
        return MapToResponse(todo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);
        if (todo == null || todo.UserId != userId) throw new UnauthorizedAccessException("Silme yetkiniz yok.");
        await _todoRepository.DeleteAsync(todo, ct);
    }

    public async Task<TodoResponse> ToggleCompleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);
        if (todo == null || todo.UserId != userId) throw new UnauthorizedAccessException("İşlem yetkisiz.");

        todo.IsCompleted = !todo.IsCompleted;
        await _todoRepository.UpdateAsync(todo, ct);
        return MapToResponse(todo);
    }

    private static TodoResponse MapToResponse(TodoItem todo)
        => new(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.DueDate);
}