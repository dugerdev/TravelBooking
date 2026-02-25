using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(UserManager<AppUser> userManager, ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DataResult<PagedResult<UserDto>>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pageNumber = request.GetValidPageNumber();
        var pageSize = request.GetValidPageSize();

        var totalCount = await _userManager.Users.CountAsync(cancellationToken);
        var users = await _userManager.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles
            });
        }

        var pagedResult = new PagedResult<UserDto>(userDtos, totalCount, pageNumber, pageSize);
        return new SuccessDataResult<PagedResult<UserDto>>(pagedResult);
    }

    public async Task<DataResult<UserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorDataResult<UserDto>(null!, "Kullanici bulunamadi.");

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd?.DateTime,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles
        };

        return new SuccessDataResult<UserDto>(userDto);
    }

    public async Task<Result> UpdateUserAsync(string userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
        {
            var userNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (userNameExists != null && userNameExists.Id != userId)
                return new ErrorResult("Bu kullanici adi zaten kullaniliyor.");

            user.UserName = dto.UserName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (emailExists != null && emailExists.Id != userId)
                return new ErrorResult("Bu email adresi zaten kullaniliyor.");

            user.Email = dto.Email;
        }

        if (dto.PhoneNumber != null)
            user.PhoneNumber = dto.PhoneNumber;

        if (dto.EmailConfirmed.HasValue)
            user.EmailConfirmed = dto.EmailConfirmed.Value;

        if (dto.LockoutEnabled.HasValue)
            user.LockoutEnabled = dto.LockoutEnabled.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} updated by admin", userId);
        return new SuccessResult("Kullanici basariyla guncellendi.");
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} deleted by admin", userId);
        return new SuccessResult("Kullanici basariyla silindi.");
    }

    public async Task<Result> LockUserAsync(string userId, DateTime? lockoutEnd, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        user.LockoutEnabled = true;
        user.LockoutEnd = lockoutEnd ?? DateTimeOffset.UtcNow.AddYears(100); // Permanent lock if no date specified

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} locked until {LockoutEnd}", userId, lockoutEnd);
        return new SuccessResult("Kullanici kilitlendi.");
    }

    public async Task<Result> UnlockUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        user.LockoutEnd = null;
        user.AccessFailedCount = 0;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} unlocked", userId);
        return new SuccessResult("Kullanici kilidi acildi.");
    }

    public async Task<Result> ActivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        user.LockoutEnabled = false;
        user.LockoutEnd = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} activated", userId);
        return new SuccessResult("Kullanici aktif edildi.");
    }

    public async Task<Result> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("User {UserId} deactivated", userId);
        return new SuccessResult("Kullanici pasif edildi.");
    }
}
