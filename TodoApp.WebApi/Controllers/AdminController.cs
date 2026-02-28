using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Services.Todo;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Sadece Admin erişebilir
public class AdminController : ControllerBase
{
    private readonly ITodoService _todoService; // Servis katmanı eklendi
    private readonly AppDbContext _context; // Kullanıcı listesi için şimdilik kalabilir

    public AdminController(ITodoService todoService, AppDbContext context)
    {
        _todoService = todoService;
        _context = context;
    }

    // ✅ YENİ: Admin artık tüm todoları sayfalı ve aramalı olarak Servis üzerinden çeker
    [HttpGet("todos")]
    public async Task<IActionResult> GetTodos(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // Servis katmanı içindeki yeni mantık, rol "Admin" olduğu için tüm kayıtları dönecektir
        var result = await _todoService.GetTodosAsync(pageNumber, pageSize, search, ct);
        return Ok(result);
    }

    // GET /api/admin/users -> Tüm kullanıcılar (Sadeleştirildi)
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(users);
    }
}