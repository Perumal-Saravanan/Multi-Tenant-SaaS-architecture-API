using MultiTenantSaaS.Core.Entities;

namespace MultiTenantSaaS.Infrastructure.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, string[] roles, Guid tenantId);
}
