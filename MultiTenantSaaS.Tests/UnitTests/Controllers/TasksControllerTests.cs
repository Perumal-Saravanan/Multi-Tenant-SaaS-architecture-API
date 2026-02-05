using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.API.Controllers;
using MultiTenantSaaS.API.DTOs;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Tests.Helpers;
using Xunit;

namespace MultiTenantSaaS.Tests.UnitTests.Controllers;

public class TasksControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly TasksController _controller;
    private readonly Guid _tenantId;
    private readonly Guid _userId;

    public TasksControllerTests()
    {
        _tenantService = new TenantService();
        _context = TestDbContextFactory.CreateInMemoryContext(_tenantService);

        var (tenant, user) = TestDbContextFactory.SeedTenantAndUser(_context);
        _tenantId = tenant.Id;
        _userId = user.Id;

        _tenantService.SetTenantContext(_tenantId, _userId.ToString());

        _controller = new TasksController(_context, _tenantService);
    }

    [Fact]
    public async Task GetTasks_ReturnsTenantSpecificTasksOnly()
    {
        // Arrange
        var task1 = new TaskItem
        {
            TenantId = _tenantId,
            Title = "Task 1",
            Description = "Description 1",
            AssignedToUserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task1);
        await _context.SaveChangesAsync();

        // Create another tenant and task
        var (otherTenant, otherUser) = TestDbContextFactory.SeedTenantAndUser(
            _context,
            "other@example.com",
            "Other Company",
            "OTHER"
        );

        var otherTask = new TaskItem
        {
            TenantId = otherTenant.Id,
            Title = "Other Task",
            Description = "Other Description",
            AssignedToUserId = otherUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(otherTask);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTasks();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tasks = okResult!.Value as IEnumerable<TaskResponse>;

        tasks.Should().NotBeNull();
        tasks.Should().HaveCount(1);
        tasks!.First().Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task CreateTask_AssignsTenantIdAutomatically()
    {
        // Arrange
        var request = new TaskRequest(
            Title: "New Task",
            Description: "New Description",
            DueDate: DateTime.UtcNow.AddDays(7),
            Priority: 2,
            CategoryId: null
        );

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var createdTask = await _context.Tasks.FirstOrDefaultAsync();
        createdTask.Should().NotBeNull();
        createdTask!.TenantId.Should().Be(_tenantId);
        createdTask.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task UpdateTask_OnlyAllowsOwnTenantTasks()
    {
        // Arrange
        var task = new TaskItem
        {
            TenantId = _tenantId,
            Title = "Original Title",
            Description = "Original Description",
            AssignedToUserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateRequest = new TaskRequest(
            Title: "Updated Title",
            Description: "Updated Description",
            DueDate: DateTime.UtcNow.AddDays(10),
            Priority: 3,
            CategoryId: null
        );

        // Act
        var result = await _controller.UpdateTask(task.Id, updateRequest);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetTask(99999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ToggleComplete_TogglesTaskCompletionStatus()
    {
        // Arrange
        var task = new TaskItem
        {
            TenantId = _tenantId,
            Title = "Task to Complete",
            Description = "Description",
            IsCompleted = false,
            AssignedToUserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ToggleComplete(task.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask!.IsCompleted.Should().BeTrue();

        // Toggle again
        await _controller.ToggleComplete(task.Id);
        updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTask_RemovesTask()
    {
        // Arrange
        var task = new TaskItem
        {
            TenantId = _tenantId,
            Title = "Task to Delete",
            Description = "Description",
            AssignedToUserId = _userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteTask(task.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteTask(99999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
