using System.Security.Claims;

namespace MultiTenantSaaS.Infrastructure.Services;

public interface ITenantService
{
    Guid GetCurrentTenantId();
    string GetCurrentUserId();
    void SetTenantContext(Guid tenantId, string userId);
}

public class TenantService : ITenantService
{
    private Guid _tenantId;
    private string _userId = string.Empty;

    public Guid GetCurrentTenantId() => _tenantId;
    
    public string GetCurrentUserId() => _userId;

    public void SetTenantContext(Guid tenantId, string userId)
    {
        _tenantId = tenantId;
        _userId = userId;
    }
}
