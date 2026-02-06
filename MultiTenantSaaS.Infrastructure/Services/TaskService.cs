using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Core.DTOs;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;

namespace MultiTenantSaaS.Infrastructure.Services;

using MultiTenantSaaS.Infrastructure.Services.Interfaces;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public TaskService(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<TaskResponse>> GetTasksAsync()
    {
        // Global Query Filter automatically filters by tenant
        var tasks = await _context.Tasks
            .AsNoTracking() // Read-only query optimization
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.DueDate,
                t.Priority,
                t.CategoryId,
                t.Category != null ? t.Category.Name : "",
                $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}",
                t.CreatedAt
            ))
            .ToListAsync();

        return tasks;
    }

    public async Task<TaskResponse?> GetTaskAsync(int id)
    {
        var task = await _context.Tasks
            .AsNoTracking() // Read-only query optimization
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return null;

        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted,
            task.DueDate,
            task.Priority,
            task.CategoryId,
            task.Category?.Name ?? "",
            $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}",
            task.CreatedAt
        );
    }

    public async Task<TaskResponse> CreateTaskAsync(TaskRequest request)
    {
        var userId = Guid.Parse(_tenantService.GetCurrentUserId());

        var task = new TaskItem
        {
            TenantId = _tenantService.GetCurrentTenantId(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            AssignedToUserId = userId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Reload the task with related data for the response
        var createdTask = await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .FirstAsync(t => t.Id == task.Id);

        return new TaskResponse(
            createdTask.Id,
            createdTask.Title,
            createdTask.Description,
            createdTask.IsCompleted,
            createdTask.DueDate,
            createdTask.Priority,
            createdTask.CategoryId,
            createdTask.Category?.Name ?? "",
            $"{createdTask.AssignedToUser.FirstName} {createdTask.AssignedToUser.LastName}",
            createdTask.CreatedAt
        );
    }

    public async Task<bool> UpdateTaskAsync(int id, TaskRequest request)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return false;

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        task.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleCompleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return false;

        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}