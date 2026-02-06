using MultiTenantSaaS.Core.Enums;

namespace MultiTenantSaaS.Core.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? Changes { get; set; } // JSON string of changes
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
