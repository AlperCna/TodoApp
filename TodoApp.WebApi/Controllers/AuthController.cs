using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Auth;
using TodoApp.Application.Services.Auth;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // URL: api/auth
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")] // POST: api/auth/register
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, ct);
        return Ok(response); // 200 OK + AuthResponse (Token dahil)
    }

    [HttpPost("login")] // POST: api/auth/login
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response); // 200 OK + AuthResponse (Token dahil)
    }
}