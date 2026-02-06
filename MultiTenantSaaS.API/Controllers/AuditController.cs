using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Core.DTOs;
using MultiTenantSaaS.Infrastructure.Services;

namespace MultiTenantSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ITenantService _tenantService;

    public AuditController(IAuditService auditService, ITenantService tenantService)
    {
        _auditService = auditService;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogResponse>>> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var logs = await _auditService.GetAuditLogsAsync(tenantId, pageNumber, pageSize);

        var response = logs.Select(log => new AuditLogResponse(
            log.Id,
            log.EntityName,
            log.EntityId,
            log.Action.ToString(),
            log.Changes,
            log.PerformedBy,
            log.PerformedAt
        ));

        return Ok(response);
    }
}
