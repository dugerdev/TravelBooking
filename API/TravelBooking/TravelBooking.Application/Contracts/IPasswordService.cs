using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface IPasswordService
{
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordRequestAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
