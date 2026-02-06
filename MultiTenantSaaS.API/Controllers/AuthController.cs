using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiTenantSaaS.Core.DTOs;
using MultiTenantSaaS.Infrastructure.Services;
using MultiTenantSaaS.Infrastructure.Services.Interfaces;

namespace MultiTenantSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);
        
        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            _logger.LogWarning("Registration failed for email: {Email} - Email already registered", request.Email);
            return BadRequest("Email already registered");
        }

        _logger.LogInformation("Registration successful for email: {Email}, TenantId: {TenantId}", request.Email, result.TenantId);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            _logger.LogWarning("Login failed for email: {Email} - Invalid credentials or account is inactive", request.Email);
            return Unauthorized("Invalid credentials or account is inactive");
        }

        _logger.LogInformation("Login successful for email: {Email}, TenantId: {TenantId}", request.Email, result.TenantId);
        return Ok(result);
    }
}
