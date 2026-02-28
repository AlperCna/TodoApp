using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Todo;
using TodoApp.Application.Services.Todo;
using TodoApp.Application.DTOs.Common;

namespace TodoApp.WebApi.Controllers;

[Authorize] // Sadece giriş yapmış kullanıcılar
[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    // ✅ GÜNCELLENDİ: Kullanıcı kendi todolarını servis üzerinden çeker
    [HttpGet]
    public async Task<IActionResult> GetMyTodos(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // Servis katmanı, rol "User" olduğu için sadece bu kullanıcının Id'si ile filtreleme yapacaktır
        var result = await _todoService.GetTodosAsync(pageNumber, pageSize, search, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoCreateRequest request, CancellationToken ct)
    {
        var result = await _todoService.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var result = await _todoService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _todoService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await _todoService.ToggleCompleteAsync(id, ct);
        return Ok(result);
    }
}