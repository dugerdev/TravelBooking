using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager,
        ILogger<RoleManagementService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DataResult<IEnumerable<RoleDto>>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = _roleManager.Roles.ToList();
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;
            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                UserCount = userCount
            });
        }

        return new SuccessDataResult<IEnumerable<RoleDto>>(roleDtos);
    }

    public async Task<DataResult<RoleDto>> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return new ErrorDataResult<RoleDto>(null!, "Rol bulunamadi.");

        var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;
        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            UserCount = userCount
        };

        return new SuccessDataResult<RoleDto>(roleDto);
    }

    public async Task<Result> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var roleExists = await _roleManager.RoleExistsAsync(dto.Name);
        if (roleExists)
            return new ErrorResult("Bu rol zaten mevcut.");

        var role = new IdentityRole(dto.Name);
        var result = await _roleManager.CreateAsync(role);
        
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Role {RoleName} created", dto.Name);
        return new SuccessResult("Rol basariyla olusturuldu.");
    }

    public async Task<Result> UpdateRoleAsync(string roleId, CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return new ErrorResult("Rol bulunamadi.");

        // Check if new name already exists
        if (dto.Name != role.Name)
        {
            var roleExists = await _roleManager.RoleExistsAsync(dto.Name);
            if (roleExists)
                return new ErrorResult("Bu rol adi zaten kullaniliyor.");
        }

        role.Name = dto.Name;
        var result = await _roleManager.UpdateAsync(role);
        
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Role {RoleId} updated", roleId);
        return new SuccessResult("Rol basariyla guncellendi.");
    }

    public async Task<Result> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null)
            return new ErrorResult("Rol bulunamadi.");

        // Check if role has users
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Count > 0)
            return new ErrorResult("Bu rolu kullanan kullanicilar var. Once kullanicilardan rolu kaldirin.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Role {RoleId} deleted", roleId);
        return new SuccessResult("Rol basariyla silindi.");
    }

    public async Task<DataResult<IEnumerable<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorDataResult<IEnumerable<string>>(null!, "Kullanici bulunamadi.");

        var roles = await _userManager.GetRolesAsync(user);
        return new SuccessDataResult<IEnumerable<string>>(roles);
    }

    public async Task<Result> AssignRoleToUserAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
            return new ErrorResult("Rol bulunamadi.");

        var isInRole = await _userManager.IsInRoleAsync(user, roleName);
        if (isInRole)
            return new ErrorResult("Kullanici zaten bu role sahip.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);
        return new SuccessResult("Rol kullaniciya atandi.");
    }

    public async Task<Result> RemoveRoleFromUserAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        var isInRole = await _userManager.IsInRoleAsync(user, roleName);
        if (!isInRole)
            return new ErrorResult("Kullanici bu role sahip degil.");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);
        return new SuccessResult("Rol kullanicidan kaldirildi.");
    }
}
