using System.ComponentModel.DataAnnotations;

namespace MultiTenantSaaS.Core.DTOs;

/// <summary>
/// Request DTO for creating or updating a task
/// </summary>
public record TaskRequest(
    [Required, MinLength(1), MaxLength(200)] string Title,
    [MaxLength(1000)] string? Description,
    DateTime? DueDate,
    [Range(1, 5)] int Priority,
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