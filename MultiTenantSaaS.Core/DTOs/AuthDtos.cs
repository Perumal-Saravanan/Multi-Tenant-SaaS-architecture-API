using System.ComponentModel.DataAnnotations;

namespace MultiTenantSaaS.Core.DTOs;

/// <summary>
/// Request DTO for user registration
/// </summary>
public record RegisterRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(8),
    RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, MaxLength(200)] string CompanyName,
    [Required, MinLength(3), MaxLength(20), RegularExpression(@"^[a-zA-Z0-9]+$", 
        ErrorMessage = "Company code must contain only letters and numbers")] 
    string CompanyCode
);

/// <summary>
/// Request DTO for user login
/// </summary>
public record LoginRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required] string Password
);

/// <summary>
/// Response DTO for authentication (login/register)
/// Contains JWT token and user information
/// </summary>
public record AuthResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    Guid TenantId,
    string[] Roles
);