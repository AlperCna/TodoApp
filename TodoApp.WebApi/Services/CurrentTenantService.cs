using System.Security.Claims;
using TodoApp.Application.Interfaces.Common;

namespace TodoApp.WebApi.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            // Token içerisindeki "tenantId" isimli claim'i ara
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenantId");

            // Eğer varsa Guid'e çevirip dön, yoksa null dön
            return string.IsNullOrEmpty(tenantIdClaim) ? null : Guid.Parse(tenantIdClaim);
        }
    }
}