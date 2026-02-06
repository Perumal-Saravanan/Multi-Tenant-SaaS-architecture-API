using MultiTenantSaaS.Core.DTOs;

namespace MultiTenantSaaS.Infrastructure.Services.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetTasksAsync();
    Task<TaskResponse?> GetTaskAsync(int id);
    Task<TaskResponse> CreateTaskAsync(TaskRequest request);
    Task<bool> UpdateTaskAsync(int id, TaskRequest request);
    Task<bool> ToggleCompleteAsync(int id);
    Task<bool> DeleteTaskAsync(int id);
}
