using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Todo;
using TodoApp.Application.Services.Todo;

namespace TodoApp.WebApi.Controllers;

[Authorize] // 4.2 Kritik Kural: Sadece giriş yapanlar erişebilir
[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet] // GET api/todos
    public async Task<IActionResult> GetMyTodos(CancellationToken ct)
    {
        var result = await _todoService.GetMyTodosAsync(ct);
        return Ok(result);
    }

    [HttpPost] // POST api/todos
    public async Task<IActionResult> Create(TodoCreateRequest request, CancellationToken ct)
    {
        var result = await _todoService.CreateAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("{id}")] // PUT api/todos/{id}
    public async Task<IActionResult> Update(Guid id, TodoUpdateRequest request, CancellationToken ct)
    {
        var result = await _todoService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")] // DELETE api/todos/{id}
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _todoService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/toggle")] // PATCH api/todos/{id}/toggle
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await _todoService.ToggleCompleteAsync(id, ct);
        return Ok(result);
    }
}