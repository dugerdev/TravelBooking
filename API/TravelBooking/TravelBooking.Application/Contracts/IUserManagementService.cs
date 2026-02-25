using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface IUserManagementService
{
    Task<DataResult<PagedResult<UserDto>>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<UserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(string userId, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> LockUserAsync(string userId, DateTime? lockoutEnd, CancellationToken cancellationToken = default);
    Task<Result> UnlockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> ActivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);
}
