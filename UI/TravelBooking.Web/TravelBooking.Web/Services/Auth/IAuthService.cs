using TravelBooking.Web.DTOs.Auth;

namespace TravelBooking.Web.Services.Auth;

public interface IAuthService
{
    Task<(bool Success, string Message, TokenResponseDto? Token)> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default);
    Task<(bool Success, string Message, TokenResponseDto? Token)> SignUpAsync(string email, string userName, string password, CancellationToken ct = default);
    Task<bool> LogoutAsync(CancellationToken ct = default);
    Task<(bool Success, TokenResponseDto? Token)> RefreshTokenAsync(CancellationToken ct = default);
    bool IsAuthenticated();
    string? GetCurrentUserId();
    string? GetCurrentUserName();
    List<string> GetCurrentUserRoles();
}
