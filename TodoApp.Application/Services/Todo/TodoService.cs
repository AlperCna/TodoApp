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
using TodoApp.Application.Exceptions;
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

    /// <summary>
    /// Kullanıcının yetkisi dahilindeki görevleri sayfalı ve aramalı olarak getirir.
    /// </summary>
    public async Task<PaginatedResult<TodoResponse>> GetTodosAsync(int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var role = _currentUserService.Role;

        // Admin her şeyi görür, User sadece kendi görevlerini.
        Guid? targetUserId = role == "Admin" ? null : userId;

        var paginatedTodos = await _todoRepository.GetTodosAsync(targetUserId, pageNumber, pageSize, search, ct);
        var mappedItems = paginatedTodos.Items.Select(MapToResponse).ToList();

        return new PaginatedResult<TodoResponse>(
            mappedItems,
            paginatedTodos.TotalCount,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Yeni görev oluşturur. 
    /// SRP: Veri temizliği (Sanitization) burada değil, Validator katmanında yapıldı.
    /// </summary>
    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var todo = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title, // Güvenli veri: Validator katmanı XSS kontrolünü yaptı
            Description = request.Description,
            DueDate = request.DueDate,
            UserId = userId,
            IsCompleted = false
        };

        await _todoRepository.AddAsync(todo, ct);
        return MapToResponse(todo);
    }

    /// <summary>
    /// Tek bir görevi getirir. Yetki kontrolü içerir.
    /// </summary>
    public async Task<TodoResponse> GetByIdMineAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        // Kayıt yoksa veya kullanıcıya ait değilse (ve admin değilse) yetki hatası dön
        var role = _currentUserService.Role;
        if (todo == null || (role != "Admin" && todo.UserId != userId))
            throw new KeyNotFoundException("İstenen görev bulunamadı veya erişim yetkiniz yok.");

        return MapToResponse(todo);
    }

    /// <summary>
    /// Mevcut görevi günceller.
    /// </summary>
    public async Task<TodoResponse> UpdateAsync(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        var role = _currentUserService.Role;
        if (todo == null || (role != "Admin" && todo.UserId != userId))
            throw new UnauthorizedAccessException("Bu görevi güncelleme yetkiniz yok.");

        // İŞ KURALI ÖRNEĞİ: Eğer görev 1 aydan daha eskiyse güncellenemesin gibi kurallar buraya gelir.

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate;

        await _todoRepository.UpdateAsync(todo, ct);
        return MapToResponse(todo);
    }

    /// <summary>
    /// Görevi siler. 
    /// BUSINESS RULE: Tamamlanmış görevlerin silinmesini engeller.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        var role = _currentUserService.Role;
        if (todo == null || (role != "Admin" && todo.UserId != userId))
            throw new UnauthorizedAccessException("Bu görevi silme yetkiniz yok.");

        // 🚀 KRİTİK İŞ KURALI (Hocanın istediği sayfa bazlı hata)
        if (todo.IsCompleted)
        {
            throw new BusinessException(
                message: "Tamamlanmış bir görevi silemezsiniz. Lütfen önce durumunu 'Devam Ediyor' olarak işaretleyin.",
                errorCode: "TODO_DELETE_FORBIDDEN_COMPLETED"
            );
        }

        await _todoRepository.DeleteAsync(todo, ct);
    }

    /// <summary>
    /// Görevin tamamlanma durumunu tersine çevirir.
    /// </summary>
    public async Task<TodoResponse> ToggleCompleteAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var todo = await _todoRepository.GetByIdAsync(id, ct);

        var role = _currentUserService.Role;
        if (todo == null || (role != "Admin" && todo.UserId != userId))
            throw new UnauthorizedAccessException("İşlem için yetkiniz bulunmamaktadır.");

        todo.IsCompleted = !todo.IsCompleted;

        await _todoRepository.UpdateAsync(todo, ct);
        return MapToResponse(todo);
    }

    /// <summary>
    /// Entity nesnesini Response DTO nesnesine dönüştürür.
    /// </summary>
    private static TodoResponse MapToResponse(TodoItem todo)
        => new(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.DueDate);
}