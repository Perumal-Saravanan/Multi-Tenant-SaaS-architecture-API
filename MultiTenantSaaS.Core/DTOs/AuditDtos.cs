namespace MultiTenantSaaS.Core.DTOs;

/// <summary>
/// Response DTO for audit log entries
/// Contains information about data changes tracked in the audit database
/// </summary>
public record AuditLogResponse(
    long Id,
    string EntityName,
    string EntityId,
    string Action,
    string? Changes,
    string PerformedBy,
    DateTime PerformedAt
);