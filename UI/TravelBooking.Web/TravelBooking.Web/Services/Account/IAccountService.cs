using TravelBooking.Web.DTOs.Account;

namespace TravelBooking.Web.Services.Account;

public interface IAccountService
{
    Task<(bool Success, string Message, UserProfileDto? Profile)> GetProfileAsync(CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateProfileAsync(UpdateProfileDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default);
}
