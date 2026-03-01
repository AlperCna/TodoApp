using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApp.Application.DTOs.Auth;
using TodoApp.Application.Interfaces.Persistence;
using TodoApp.Application.Services.Auth;

namespace TodoApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantRepository _tenantRepository;

    public AuthController(IAuthService authService, ITenantRepository tenantRepository)
    {
        _authService = authService;
        _tenantRepository = tenantRepository;
    }

    // --- MEVCUT YEREL GİRİŞ METOTLARI ---

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Ok(response);
    }

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

    // --- 🛡️ YENİ: ENTERPRISE SSO (MICROSOFT & GOOGLE) METOTLARI ---

    [HttpGet("login-microsoft")]
    public IActionResult LoginWithMicrosoft()
    {
        // Azure Portal'daki yapılandırmayı tetikler
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalCallback") };
        return Challenge(properties, "Microsoft");
    }

    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle()
    {
        // Google Cloud tarafındaki yapılandırmayı tetikler
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalCallback") };
        return Challenge(properties, "Google");
    }

    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback(CancellationToken ct)
    {
        // 1. Dış servisten (Microsoft/Google) gelen kimlik bilgilerini oku
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded) return BadRequest("External authentication failed.");

        // 2. Kullanıcı bilgilerini claims üzerinden al
        var externalId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = result.Principal.FindFirstValue(ClaimTypes.Email) ?? result.Principal.FindFirstValue("preferred_username");
        var provider = result.Properties.Items[".AuthScheme"]; // 'Microsoft' veya 'Google'

        if (string.IsNullOrEmpty(email)) return BadRequest("Email not provided by external service.");

        // 🧠 Mülakatın Sırrı: Tenant Binding (Domain üzerinden şirkete bağlama)
        var domain = email.Split('@')[1]; // Örn: fsm.edu.tr

        // 3. AuthService üzerinden bu kullanıcıyı sisteme kaydet/giriş yaptır
        // Bu metod içinde 'domain' üzerinden TenantId bulunup kullanıcıya atanacak.
        var authResponse = await _authService.HandleExternalLoginAsync(new ExternalLoginDto
        {
            Email = email,
            ExternalId = externalId!,
            Provider = provider!,
            Domain = domain
        }, ct);

        // 4. Tarayıcıyı Angular uygulamasına (localhost:4200) token ile geri gönder
        // Angular tarafında bu token yakalanıp LocalStorage'a atılacak.
        return Redirect($"http://localhost:4200/sso-success?accessToken={authResponse.Token}&refreshToken={authResponse.RefreshToken}");
    }
}