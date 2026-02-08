using System.Security.Claims;
using TodoApp.Application.Interfaces.Common;

namespace TodoApp.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Token içindeki 'sub' (Subject) claim'ini okuyup Guid'e çeviriyoruz
    public Guid? UserId
    {
        get
        {
            // HttpContext üzerinden NameIdentifier claim'ini çekiyoruz
            var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Eğer id boşsa null, doluysa Guid'e çevirip döndürüyoruz
            return string.IsNullOrEmpty(id) ? null : Guid.Parse(id);
        }
    }
}