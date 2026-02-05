using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Tests.Helpers;
using Xunit;

namespace MultiTenantSaaS.Tests.IntegrationTests;

public class TenantIsolationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public TenantIsolationTests()
    {
        _tenantService = new TenantService();
        _context = TestDbContextFactory.CreateInMemoryContext(_tenantService);
    }

    [Fact]
    public async Task MultiTenant_TasksAreIsolatedBetweenTenants()
    {
        // Arrange - Create Tenant A
        var (tenantA, userA) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "usera@tenanta.com",
            "Tenant A",
            "TENA"
        );

        _tenantService.SetTenantContext(tenantA.Id, userA.Id.ToString());

        var taskA = new TaskItem
        {
            TenantId = tenantA.Id,
            Title = "Task A",
            Description = "Belongs to Tenant A",
            AssignedToUserId = userA.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskA);
        await _context.SaveChangesAsync();

        // Arrange - Create Tenant B
        var (tenantB, userB) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "userb@tenantb.com",
            "Tenant B",
            "TENB"
        );

        _tenantService.SetTenantContext(tenantB.Id, userB.Id.ToString());

        var taskB = new TaskItem
        {
            TenantId = tenantB.Id,
            Title = "Task B",
            Description = "Belongs to Tenant B",
            AssignedToUserId = userB.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskB);
        await _context.SaveChangesAsync();

        // Act - Query as Tenant A
        _tenantService.SetTenantContext(tenantA.Id, userA.Id.ToString());
        var tasksForTenantA = await _context.Tasks.ToListAsync();

        // Assert - Tenant A should only see their task
        tasksForTenantA.Should().HaveCount(1);
        tasksForTenantA.First().Title.Should().Be("Task A");

        // Act - Query as Tenant B
        _tenantService.SetTenantContext(tenantB.Id, userB.Id.ToString());
        var tasksForTenantB = await _context.Tasks.ToListAsync();

        // Assert - Tenant B should only see their task
        tasksForTenantB.Should().HaveCount(1);
        tasksForTenantB.First().Title.Should().Be("Task B");
    }

    [Fact]
    public async Task MultiTenant_CannotAccessOtherTenantData()
    {
        // Arrange
        var (tenantA, userA) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "admin@companya.com",
            "Company A",
            "COMPA"
        );

        var (tenantB, userB) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "admin@companyb.com",
            "Company B",
            "COMPB"
        );

        _tenantService.SetTenantContext(tenantA.Id, userA.Id.ToString());

        var taskA = new TaskItem
        {
            TenantId = tenantA.Id,
            Title = "Confidential Task A",
            Description = "Secret information",
            AssignedToUserId = userA.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(taskA);
        await _context.SaveChangesAsync();

        // Act - Try to access as Tenant B
        _tenantService.SetTenantContext(tenantB.Id, userB.Id.ToString());
        var taskById = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskA.Id);

        // Assert - Tenant B cannot see Tenant A's task
        taskById.Should().BeNull();
    }

    [Fact]
    public async Task GlobalQueryFilter_AutomaticallyFiltersByTenant()
    {
        // Arrange - Create multiple tenants with tasks
        var (tenant1, user1) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "user1@tenant1.com",
            "Tenant 1",
            "TEN1"
        );

        var (tenant2, user2) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "user2@tenant2.com",
            "Tenant 2",
            "TEN2"
        );

        var (tenant3, user3) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "user3@tenant3.com",
            "Tenant 3",
            "TEN3"
        );

        // Add tasks for each tenant
        _tenantService.SetTenantContext(tenant1.Id, user1.Id.ToString());
        _context.Tasks.Add(new TaskItem
        {
            TenantId = tenant1.Id,
            Title = "Task 1",
            AssignedToUserId = user1.Id,
            CreatedAt = DateTime.UtcNow
        });

        _tenantService.SetTenantContext(tenant2.Id, user2.Id.ToString());
        _context.Tasks.Add(new TaskItem
        {
            TenantId = tenant2.Id,
            Title = "Task 2",
            AssignedToUserId = user2.Id,
            CreatedAt = DateTime.UtcNow
        });

        _tenantService.SetTenantContext(tenant3.Id, user3.Id.ToString());
        _context.Tasks.Add(new TaskItem
        {
            TenantId = tenant3.Id,
            Title = "Task 3",
            AssignedToUserId = user3.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Act & Assert - Each tenant sees only their own tasks
        _tenantService.SetTenantContext(tenant1.Id, user1.Id.ToString());
        var tasks1 = await _context.Tasks.ToListAsync();
        tasks1.Should().HaveCount(1);
        tasks1.First().Title.Should().Be("Task 1");

        _tenantService.SetTenantContext(tenant2.Id, user2.Id.ToString());
        var tasks2 = await _context.Tasks.ToListAsync();
        tasks2.Should().HaveCount(1);
        tasks2.First().Title.Should().Be("Task 2");

        _tenantService.SetTenantContext(tenant3.Id, user3.Id.ToString());
        var tasks3 = await _context.Tasks.ToListAsync();
        tasks3.Should().HaveCount(1);
        tasks3.First().Title.Should().Be("Task 3");
    }

    [Fact]
    public async Task MultiTenant_UsersAreIsolatedBetweenTenants()
    {
        // Arrange
        var (tenantA, userA) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "usera@tenanta.com",
            "Tenant A",
            "TENA"
        );

        var (tenantB, userB) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "userb@tenantb.com",
            "Tenant B",
            "TENB"
        );

        // Act - Query users as Tenant A
        _tenantService.SetTenantContext(tenantA.Id, userA.Id.ToString());
        var usersForTenantA = await _context.Users.ToListAsync();

        // Assert - Tenant A should only see their user
        usersForTenantA.Should().HaveCount(1);
        usersForTenantA.First().Email.Should().Be("usera@tenanta.com");

        // Act - Query users as Tenant B
        _tenantService.SetTenantContext(tenantB.Id, userB.Id.ToString());
        var usersForTenantB = await _context.Users.ToListAsync();

        // Assert - Tenant B should only see their user
        usersForTenantB.Should().HaveCount(1);
        usersForTenantB.First().Email.Should().Be("userb@tenantb.com");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
