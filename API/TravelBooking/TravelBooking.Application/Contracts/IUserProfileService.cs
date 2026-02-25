using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface IUserProfileService
{
    Task<DataResult<UserProfileDto>> GetCurrentUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<ReservationDto>>> GetUserReservationsAsync(string userId, PagedRequest request, CancellationToken cancellationToken = default);
}
