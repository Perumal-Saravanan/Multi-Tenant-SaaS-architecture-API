namespace MultiTenantSaaS.API.DTOs;

/// <summary>
/// Request DTO for creating or updating a task
/// </summary>
public record TaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    int? CategoryId
);

/// <summary>
/// Response DTO for task information
/// Includes computed properties like CategoryName and AssignedToName
/// </summary>
public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueDate,
    int Priority,
    int? CategoryId,
    string CategoryName,
    string AssignedToName,
    DateTime CreatedAt
);
