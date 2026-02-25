using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface IRoleManagementService
{
    Task<DataResult<IEnumerable<RoleDto>>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<DataResult<RoleDto>> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken = default);
    Task<Result> CreateRoleAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateRoleAsync(string roleId, CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> AssignRoleToUserAsync(string userId, string roleName, CancellationToken cancellationToken = default);
    Task<Result> RemoveRoleFromUserAsync(string userId, string roleName, CancellationToken cancellationToken = default);
}
