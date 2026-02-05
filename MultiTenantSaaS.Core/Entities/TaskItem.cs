using MultiTenantSaaS.Core.Interfaces;

namespace MultiTenantSaaS.Core.Entities;

public class TaskItem : ITenantEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High
    
    // Foreign keys
    public Guid AssignedToUserId { get; set; }
    public int? CategoryId { get; set; }
    
    // Auditable properties
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Navigation properties
    public User AssignedToUser { get; set; } = null!;
    public Category? Category { get; set; }
}
