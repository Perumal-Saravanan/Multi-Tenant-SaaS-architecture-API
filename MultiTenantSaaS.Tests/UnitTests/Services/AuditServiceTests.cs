using FluentAssertions;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Core.Enums;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Tests.Helpers;
using Xunit;

namespace MultiTenantSaaS.Tests.UnitTests.Services;

public class AuditServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ITenantService _tenantService;

    public AuditServiceTests()
    {
        _tenantService = new TenantService();
        _context = TestDbContextFactory.CreateInMemoryContext(_tenantService);
        _auditService = new AuditService(_context);
    }

    [Fact]
    public async Task LogAuditsAsync_CreatesAuditLogEntries()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                TenantId = tenantId,
                EntityName = "TaskItem",
                EntityId = "123",
                Action = AuditAction.Insert,
                Changes = "{\"Title\":\"New Task\"}",
                PerformedBy = "test-user-id",
                PerformedAt = DateTime.UtcNow
            }
        };

        // Act
        await _auditService.LogAuditsAsync(auditLogs);
        
        // Set tenant context to query the audit logs
        _tenantService.SetTenantContext(tenantId, "test-user-id");

        // Assert
        var savedLog = _context.AuditLogs.FirstOrDefault();
        savedLog.Should().NotBeNull();
        savedLog!.TenantId.Should().Be(tenantId);
        savedLog.EntityName.Should().Be("TaskItem");
        savedLog.EntityId.Should().Be("123");
        savedLog.Action.Should().Be(AuditAction.Insert);
        savedLog.PerformedBy.Should().Be("test-user-id");
        savedLog.Changes.Should().Be("{\"Title\":\"New Task\"}");
    }

    [Fact]
    public async Task LogAuditsAsync_HandlesMultipleEntries()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                TenantId = tenantId,
                EntityName = "TaskItem",
                EntityId = "1",
                Action = AuditAction.Insert,
                PerformedBy = "user-1",
                PerformedAt = DateTime.UtcNow
            },
            new AuditLog
            {
                TenantId = tenantId,
                EntityName = "TaskItem",
                EntityId = "2",
                Action = AuditAction.Delete,
                PerformedBy = "user-2",
                PerformedAt = DateTime.UtcNow
            }
        };

        // Act
        await _auditService.LogAuditsAsync(auditLogs);
        
        // Set tenant context to query the audit logs
        _tenantService.SetTenantContext(tenantId, "user-1");

        // Assert
        var savedLogs = _context.AuditLogs.ToList();
        savedLogs.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ReturnsFilteredByTenant()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var auditLogs = new List<AuditLog>
        {
            new AuditLog
            {
                TenantId = tenant1,
                EntityName = "TaskItem",
                EntityId = "1",
                Action = AuditAction.Insert,
                PerformedBy = "user-1",
                PerformedAt = DateTime.UtcNow
            },
            new AuditLog
            {
                TenantId = tenant2,
                EntityName = "TaskItem",
                EntityId = "2",
                Action = AuditAction.Insert,
                PerformedBy = "user-2",
                PerformedAt = DateTime.UtcNow
            }
        };

        await _auditService.LogAuditsAsync(auditLogs);

        // Act
        var result = await _auditService.GetAuditLogsAsync(tenant1);

        // Assert
        result.Should().HaveCount(1);
        result.First().TenantId.Should().Be(tenant1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_SupportsPagination()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var auditLogs = Enumerable.Range(1, 25).Select(i => new AuditLog
        {
            TenantId = tenantId,
            EntityName = "TaskItem",
            EntityId = i.ToString(),
            Action = AuditAction.Insert,
            PerformedBy = "user",
            PerformedAt = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        await _auditService.LogAuditsAsync(auditLogs);

        // Act
        var page1 = await _auditService.GetAuditLogsAsync(tenantId, pageNumber: 1, pageSize: 10);
        var page2 = await _auditService.GetAuditLogsAsync(tenantId, pageNumber: 2, pageSize: 10);

        // Assert
        page1.Should().HaveCount(10);
        page2.Should().HaveCount(10);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
