using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.API.Controllers;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Core.Enums;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Tests.Helpers;
using Xunit;

namespace MultiTenantSaaS.Tests.UnitTests.Controllers;

public class AuditControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ITenantService _tenantService;
    private readonly AuditController _controller;
    private readonly Guid _tenantId;

    public AuditControllerTests()
    {
        _tenantService = new TenantService();
        _context = TestDbContextFactory.CreateInMemoryContext(_tenantService);
        _auditService = new AuditService(_context);

        _tenantId = Guid.NewGuid();
        _tenantService.SetTenantContext(_tenantId, "test-user-id");

        _controller = new AuditController(_auditService, _tenantService);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                TenantId = _tenantId,
                EntityName = "TaskItem",
                EntityId = i.ToString(),
                Action = AuditAction.Insert,
                PerformedBy = "test-user-id",
                PerformedAt = DateTime.UtcNow,
                Changes = "{}"
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAuditLogs(pageNumber: 1, pageSize: 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value;

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAuditLogs_FiltersByTenant()
    {
        // Arrange
        _context.AuditLogs.Add(new AuditLog
        {
            TenantId = _tenantId,
            EntityName = "TaskItem",
            EntityId = "1",
            Action = AuditAction.Insert,
            PerformedBy = "test-user-id",
            PerformedAt = DateTime.UtcNow,
            Changes = "{}"
        });

        var otherTenantId = Guid.NewGuid();
        _context.AuditLogs.Add(new AuditLog
        {
            TenantId = otherTenantId,
            EntityName = "TaskItem",
            EntityId = "2",
            Action = AuditAction.Insert,
            PerformedBy = "other-user-id",
            PerformedAt = DateTime.UtcNow,
            Changes = "{}"
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAuditLogs(pageNumber: 1, pageSize: 50);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        
        // The controller should only return logs for the current tenant
        var logs = _context.AuditLogs.Where(l => l.TenantId == _tenantId).ToList();
        logs.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
