using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Auth;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Services.Auth;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // URL: api/auth
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantRepository _tenantRepository;

    public AuthController(IAuthService authService, ITenantRepository tenantRepository)
    {
        _authService = authService;
        _tenantRepository = tenantRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        // AuthService içindeki yeni mantık hem Access hem Refresh Token döner
        var response = await _authService.RegisterAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        // Login anında artık veritabanına Refresh Token kaydedilir ve geri döner
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response);
    }

    // ✅ YENİ: Sessiz Yenileme (Silent Refresh) Endpoint'i
    // Angular Interceptor, 401 hatası aldığında buraya istek atacak
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Eğer Refresh Token geçersizse veya süresi dolmuşsa 401 dönüyoruz
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
        var tenants = await _tenantRepository.GetAllAsync(ct);
        var tenantNames = tenants.Select(t => t.Name).ToList();
        return Ok(tenantNames);
    }
}