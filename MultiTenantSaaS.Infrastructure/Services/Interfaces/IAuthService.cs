using MultiTenantSaaS.Core.DTOs;

namespace MultiTenantSaaS.Infrastructure.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
