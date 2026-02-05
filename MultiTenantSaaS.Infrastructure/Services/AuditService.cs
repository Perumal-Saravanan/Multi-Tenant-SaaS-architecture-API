using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;

namespace MultiTenantSaaS.Infrastructure.Services;

public interface IAuditService
{
    Task LogAuditsAsync(IEnumerable<AuditLog> auditLogs);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50);
}

public class AuditService : IAuditService
{
    private readonly AuditDbContext _auditDbContext;

    public AuditService(AuditDbContext auditDbContext)
    {
        _auditDbContext = auditDbContext;
    }

    public async Task LogAuditsAsync(IEnumerable<AuditLog> auditLogs)
    {
        await _auditDbContext.AuditLogs.AddRangeAsync(auditLogs);
        await _auditDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50)
    {
        return await Task.Run(() =>
        {
            return _auditDbContext.AuditLogs
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.PerformedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        });
    }
}
