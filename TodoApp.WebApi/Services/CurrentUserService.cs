using System.Security.Claims;
using TodoApp.Application.Interfaces.Common;

namespace TodoApp.WebApi.Services;

using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? Guid.Parse(id) : null;
        }
    }

    // 🔑 YENİ: Token içindeki Role claim'ini okur
    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
}