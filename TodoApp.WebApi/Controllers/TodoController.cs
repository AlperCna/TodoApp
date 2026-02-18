using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Todo;
using TodoApp.Application.Services.Todo;
using TodoApp.Application.DTOs.Common; // PaginatedResult için gerekli

namespace TodoApp.WebApi.Controllers;

[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>
    /// Kullanıcının görevlerini sayfalı olarak getirir.
    /// Örn: GET api/todo?pageNumber=1&pageSize=10
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyTodos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // Sayfalama parametrelerini servise iletiyoruz
        var result = await _todoService.GetMyTodosAsync(pageNumber, pageSize, ct);

        // Dönen result artık sadece liste değil; Items, TotalCount ve Page bilgilerini içerir.
        return Ok(result);
    }

    [HttpPost] // POST api/todo
    public async Task<IActionResult> Create(TodoCreateRequest request, CancellationToken ct)
    {
        var result = await _todoService.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")] // PUT api/todo/{id}
    public async Task<IActionResult> Update(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var result = await _todoService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")] // DELETE api/todo/{id}
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Repository'de yaptığımız Soft Delete değişikliği sayesinde bu metot 
        // veriyi DB'den silmez, sadece IsDeleted=1 olarak işaretler.
        await _todoService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/toggle")] // PATCH api/todo/{id}/toggle
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await _todoService.ToggleCompleteAsync(id, ct);
        return Ok(result);
    }
}