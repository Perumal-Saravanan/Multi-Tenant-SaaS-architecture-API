using System.ComponentModel.DataAnnotations;

namespace MultiTenantSaaS.Core.DTOs;

/// <summary>
/// Request DTO for creating or updating a category
/// </summary>
public record CategoryRequest(
    [Required, MinLength(1), MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Required, RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF5733)")] 
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