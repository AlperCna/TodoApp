using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // ✅ sadece Admin erişebilir
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/admin/users  -> tüm kullanıcılar
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
                u.Role
            })
            .ToListAsync(ct);

        return Ok(users);
    }

    // GET /api/admin/todos -> tüm kullanıcıların todoları (soft delete filtresi sende global filter ile zaten eler)
    [HttpGet("todos")]
    public async Task<IActionResult> GetTodos(CancellationToken ct)
    {
        var todos = await _context.TodoItems
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.IsCompleted,
                t.CreatedAt,
                t.UserId
            })
            .ToListAsync(ct);

        return Ok(todos);
    }
}
