using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Services.Todo;
using TodoApp.Application.Services.Admin; // Yeni servis namespace'iniz

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Sadece Admin erişebilir
public class AdminController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly IAdminService _adminService; // ✅ DbContext gitti, Service geldi

    public AdminController(ITodoService todoService, IAdminService adminService)
    {
        _todoService = todoService;
        _adminService = adminService;
    }

    // Admin artık tüm todoları sayfalı ve aramalı olarak Servis üzerinden çeker (Üst kısım - Değişmedi)
    [HttpGet("todos")]
    public async Task<IActionResult> GetTodos(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _todoService.GetTodosAsync(pageNumber, pageSize, search, ct);
        return Ok(result);
    }

    // GET /api/admin/users -> Tüm kullanıcılar artık tertemiz!
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        // Tüm logic AdminService içinde, Controller sadece sonucu döner
        var users = await _adminService.GetUsersSummaryAsync(ct);
        return Ok(users);
    }
}