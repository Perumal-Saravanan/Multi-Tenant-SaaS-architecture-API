namespace MultiTenantSaaS.API.DTOs;

/// <summary>
/// Request DTO for creating or updating a category
/// </summary>
public record CategoryRequest(
    string Name,
    string? Description,
    string Color
);

/// <summary>
/// Response DTO for category information
/// Includes computed TaskCount property
/// </summary>
public record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    string Color,
    int TaskCount
);
