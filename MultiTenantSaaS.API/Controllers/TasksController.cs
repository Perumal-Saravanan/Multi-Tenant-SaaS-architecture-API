using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.API.DTOs;
using MultiTenantSaaS.Core.Entities;
using MultiTenantSaaS.Infrastructure.Data;
using MultiTenantSaaS.Infrastructure.Services;

namespace MultiTenantSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public TasksController(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks()
    {
        // Global Query Filter automatically filters by tenant
        var tasks = await _context.Tasks
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

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        return Ok(new TaskResponse(
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
        ));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(TaskRequest request)
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

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskRequest request)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        task.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
