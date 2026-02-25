using TravelBooking.Application.Common;

namespace TravelBooking.Application.Contracts;

public interface IEmailVerificationService
{
    Task<Result> SendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default);
}
