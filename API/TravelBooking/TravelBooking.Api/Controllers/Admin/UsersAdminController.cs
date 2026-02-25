using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace TravelBooking.Api.Controllers.Admin;

/// <summary>
/// Kullanıcı yönetimi için Admin API endpoint'leri.
/// Tüm endpoint'ler Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersAdminController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<UsersAdminController> _logger;

    public UsersAdminController(
        IUserManagementService userManagementService,
        UserManager<AppUser> userManager,
        ILogger<UsersAdminController> logger)
    {
        _userManagementService = userManagementService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<DataResult<PagedResult<UserDto>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetAllUsersAsync(request, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataResult<UserDto>>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = dto.EmailConfirmed
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "User");
        return Ok(new SuccessResult("Kullanıcı başarıyla oluşturuldu."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(string id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UpdateUserAsync(id, dto, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.DeleteUserAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/lock")]
    public async Task<ActionResult<Result>> Lock(string id, [FromBody] LockUserRequest? request, CancellationToken cancellationToken)
    {
        var lockoutEnd = request?.LockoutEnd;
        var result = await _userManagementService.LockUserAsync(id, lockoutEnd, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<Result>> Unlock(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UnlockUserAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<Result>> Activate(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.ActivateUserAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<Result>> Deactivate(string id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.DeactivateUserAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{userId}/make-admin")]
    public async Task<IActionResult> MakeAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }
}

public class LockUserRequest
{
    public DateTime? LockoutEnd { get; set; }
}