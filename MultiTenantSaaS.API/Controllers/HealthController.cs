using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantSaaS.Infrastructure.Data;

namespace MultiTenantSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ApplicationDbContext context,
        ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "Multi-Tenant SaaS API",
                Version = "1.0.0",
                Checks = new
                {
                    Database = await CheckDatabaseHealth()
                }
            };

            var allHealthy = healthStatus.Checks.Database == "Healthy";

            if (allHealthy)
            {
                return Ok(healthStatus);
            }
            else
            {
                return StatusCode(503, healthStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    private async Task<string> CheckDatabaseHealth()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return "Healthy";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Main database health check failed");
            return "Unhealthy";
        }
    }
}
