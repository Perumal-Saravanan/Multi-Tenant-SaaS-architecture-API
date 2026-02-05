using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Core.Enums;
using MultiTenantSaaS.Infrastructure.Services;
using System.Text.Json;

namespace MultiTenantSaaS.Infrastructure.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ITenantService _tenantService;
    private readonly IAuditService _auditService;

    public AuditInterceptor(ITenantService tenantService, IAuditService auditService)
    {
        _tenantService = tenantService;
        _auditService = auditService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await CaptureAuditLogs(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task CaptureAuditLogs(DbContext context)
    {
        var auditEntries = new List<AuditLog>();
        var tenantId = _tenantService.GetCurrentTenantId();
        var userId = _tenantService.GetCurrentUserId();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Only audit Insert and Delete operations
            if (entry.State != EntityState.Added && entry.State != EntityState.Deleted)
                continue;

            // Skip auditing the AuditLog entity itself
            if (entry.Entity is AuditLog)
                continue;

            var entityName = entry.Entity.GetType().Name;
            var entityId = GetPrimaryKeyValue(entry);
            var action = entry.State == EntityState.Added ? AuditAction.Insert : AuditAction.Delete;

            var changes = new Dictionary<string, object?>();
            
            if (entry.State == EntityState.Added)
            {
                foreach (var property in entry.Properties)
                {
                    changes[property.Metadata.Name] = property.CurrentValue;
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var property in entry.Properties)
                {
                    changes[property.Metadata.Name] = property.OriginalValue;
                }
            }

            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                Changes = JsonSerializer.Serialize(changes),
                PerformedBy = userId,
                PerformedAt = DateTime.UtcNow
            };

            auditEntries.Add(auditLog);
        }

        if (auditEntries.Any())
        {
            await _auditService.LogAuditsAsync(auditEntries);
        }
    }

    private string GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? "null");

        return string.Join(",", keyValues);
    }
}
