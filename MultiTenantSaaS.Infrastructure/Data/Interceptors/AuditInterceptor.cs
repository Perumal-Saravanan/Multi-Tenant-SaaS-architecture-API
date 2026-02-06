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

    public AuditInterceptor(ITenantService tenantService)
    {
        _tenantService = tenantService;
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

            // Determine effective TenantId
            Guid? effectiveTenantId = tenantId == Guid.Empty ? null : tenantId;

            // If we are creating a new Tenant, use its ID for the audit log
            if (entry.Entity is Tenant tenant && entry.State == EntityState.Added)
            {
                effectiveTenantId = tenant.Id;
            }


            // Skip auditing the AuditLog entity itself to avoid infinite loops if we were saving separately
            // Even though we are adding to the same context now, it is good practice.
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
                TenantId = effectiveTenantId,
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
            // Directly add to the context. 
            // Since we are in the SavingChanges interceptor, these will be part of the current transaction.
            // IMPORTANT: Do NOT call SaveChangesAsync here, as it would cause infinite recursion.
            await context.AddRangeAsync(auditEntries);
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
