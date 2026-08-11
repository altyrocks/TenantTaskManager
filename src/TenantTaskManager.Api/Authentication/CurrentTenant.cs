using System.Security.Claims;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Api.Authentication;

public sealed class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid TenantId
    {
        get
        {
            var tenantIdValue = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");

            if (!Guid.TryParse(tenantIdValue, out var tenantId))
            {
                throw new UnauthorizedAccessException("The authenticated user does not have a valid tenant claim.");
            }

            return tenantId;
        }
    }
}