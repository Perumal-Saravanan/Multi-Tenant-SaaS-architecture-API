namespace MultiTenantSaaS.API.DTOs;

/// <summary>
/// Request DTO for user registration
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string CompanyName,
    string CompanyCode
);

/// <summary>
/// Request DTO for user login
/// </summary>
public record LoginRequest(
    string Email,
    string Password
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
