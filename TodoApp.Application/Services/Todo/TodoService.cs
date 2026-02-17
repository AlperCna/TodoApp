using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TodoApp.Application.DTOs.Todo;
using TodoApp.Application.Interfaces.Common;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Services.Todo;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Services.Todo;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICurrentUserService _currentUserService;

    public TodoService(ITodoRepository todoRepository, ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var todo = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate, 
            UserId = userId,
            IsCompleted = false
        };

        await _todoRepository.AddAsync(todo, ct);
        return MapToResponse(todo);
    }

    public async Task<IEnumerable<TodoResponse>> GetMyTodosAsync(CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todos = await _todoRepository.GetUserTodosAsync(userId, ct);
        return todos.Select(MapToResponse);
    }

    public async Task<TodoResponse> GetByIdMineAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        if (todo == null || todo.UserId != userId)
            throw new UnauthorizedAccessException("Yetkisiz erişim.");

        return MapToResponse(todo);
    }

    public async Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        if (todo == null || todo.UserId != userId)
            throw new UnauthorizedAccessException("Güncelleme yetkiniz yok.");

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate; 

        await _todoRepository.UpdateAsync(todo, ct);
        return MapToResponse(todo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        if (todo == null || todo.UserId != userId)
            throw new UnauthorizedAccessException("Silme yetkiniz yok.");

        await _todoRepository.DeleteAsync(todo, ct);
    }

    public async Task<TodoResponse> ToggleCompleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        if (todo == null || todo.UserId != userId)
            throw new UnauthorizedAccessException("İşlem yetkisiz.");

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