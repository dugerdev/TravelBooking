using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Api.Controllers.Admin;

/// <summary>
/// Rol yönetimi için Admin API endpoint'leri.
/// Tüm endpoint'ler Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/admin/roles")]
[Authorize(Roles = "Admin")]
public sealed class RolesAdminController : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService;
    private readonly ILogger<RolesAdminController> _logger;

    public RolesAdminController(IRoleManagementService roleManagementService, ILogger<RolesAdminController> logger)
    {
        _roleManagementService = roleManagementService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<DataResult<IEnumerable<RoleDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetAllRolesAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataResult<RoleDto>>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetRoleByIdAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result>> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.CreateRoleAsync(dto, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(string id, [FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.UpdateRoleAsync(id, dto, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(string id, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.DeleteRoleAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("users/{userId}/roles")]
    public async Task<ActionResult<DataResult<IEnumerable<string>>>> GetUserRoles(string userId, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.GetUserRolesAsync(userId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("users/{userId}/roles")]
    public async Task<ActionResult<Result>> AssignRole(string userId, [FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.AssignRoleToUserAsync(userId, dto.RoleName, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<ActionResult<Result>> RemoveRole(string userId, string roleName, CancellationToken cancellationToken)
    {
        var result = await _roleManagementService.RemoveRoleFromUserAsync(userId, roleName, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
