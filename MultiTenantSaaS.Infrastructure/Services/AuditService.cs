using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAuditsAsync(IEnumerable<AuditLog> auditLogs)
    {
        await _context.AuditLogs.AddRangeAsync(auditLogs);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .AsNoTracking() // Read-only query optimization
            .IgnoreQueryFilters() // Bypass global tenant filter since we explicitly filter by tenantId parameter
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.PerformedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
