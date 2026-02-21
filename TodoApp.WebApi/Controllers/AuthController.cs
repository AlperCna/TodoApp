using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.DTOs.Auth;
using TodoApp.Application.Interfaces.Persistence; // ITenantRepository için
using TodoApp.Application.Services.Auth;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // URL: api/auth
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantRepository _tenantRepository; // ✅ Yeni eklendi

    public AuthController(IAuthService authService, ITenantRepository tenantRepository)
    {
        _authService = authService;
        _tenantRepository = tenantRepository; // ✅ Inject edildi
    }

    [HttpPost("register")] // POST: api/auth/register
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        // AuthService içindeki akıllı mantık sayesinde TenantId otomatik yönetiliyor
        var response = await _authService.RegisterAsync(request, ct);
        return Ok(response); // 200 OK + AuthResponse (Token dahil)
    }

    [HttpPost("login")] // POST: api/auth/login
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response); // 200 OK + AuthResponse (Token dahil)
    }

    // ✅ YENİ: Frontend'deki açılır liste (nz-select) için şirket isimlerini döner
    [HttpGet("tenants")] // GET: api/auth/tenants
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
        // ITenantRepository üzerinden tüm şirketleri çekiyoruz
        var tenants = await _tenantRepository.GetAllAsync(ct);

        // Sadece isimlerini (Name) liste halinde dönmek frontend için yeterlidir
        var tenantNames = tenants.Select(t => t.Name).ToList();

        return Ok(tenantNames);
    }
}