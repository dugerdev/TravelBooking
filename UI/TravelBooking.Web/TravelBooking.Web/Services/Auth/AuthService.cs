using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Auth;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.Services.TravelBookingApi;
using Microsoft.Extensions.Options;

namespace TravelBooking.Web.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITravelBookingApiClient _api;
    private readonly ICookieHelper _cookieHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ITravelBookingApiClient api, ICookieHelper cookieHelper, IOptions<Configuration.TravelBookingApiOptions> options, ILogger<AuthService> logger)
    {
        _api = api;
        _cookieHelper = cookieHelper;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, TokenResponseDto? Token)> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
                return (false, "Username/email and password are required.", null);

            var body = new LoginRequestDto { UserNameOrEmail = userNameOrEmail, Password = password };
            var token = await _api.PostUnauthAsync<TokenResponseDto>(ApiEndpoints.AuthLogin, body, ct);
            if (token == null)
                return (false, "Login failed. Enter valid email/username and password.", null);
            
            //Örnek: Access ve refresh token'lar cookie'ye yazilir; HttpOnly ile XSS'e karsi korunur
            await _cookieHelper.SetAuthCookiesAsync(token, ct);
            return (true, "Giris basarili.", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {UserNameOrEmail}. Error: {Message}", userNameOrEmail, ex.Message);
            return (false, "An error occurred during login. Please try again.", null);
        }
    }

    public async Task<(bool Success, string Message, TokenResponseDto? Token)> SignUpAsync(string email, string userName, string password, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return (false, "Email, username and password are required.", null);

            if (password.Length < 6)
                return (false, "Sifre en az 6 karakter olmalidir.", null);

            var body = new SignupRequestDto { Email = email, UserName = userName, Password = password };
            var token = await _api.PostUnauthAsync<TokenResponseDto>(ApiEndpoints.AuthSignup, body, ct);
            if (token == null)
                return (false, "Registration failed. Email or username may already be in use.", null);
            
            await _cookieHelper.SetAuthCookiesAsync(token, ct);
            return (true, "Kayit basarili.", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Signup failed for email: {Email}. Error: {Message}", email, ex.Message);
            return (false, "An error occurred during registration. Please try again.", null);
        }
    }

    public async Task<bool> LogoutAsync(CancellationToken ct = default)
    {
        var refresh = _cookieHelper.GetRefreshToken();
        if (!string.IsNullOrEmpty(refresh))
        {
            var body = new LogoutRequestDto { RefreshToken = refresh };
            try
            {
                await _api.PostNoContentAsync(ApiEndpoints.AuthLogout, body, ct);
            }
            catch
            {
                /* best effort */
            }
        }
        _cookieHelper.ClearAuthCookies();
        return true;
    }

    //Örnek: Refresh token ile yeni access token alir; access token suresi doldugunda API cagrilarindan once cagrilir
    public async Task<(bool Success, TokenResponseDto? Token)> RefreshTokenAsync(CancellationToken ct = default)
    {
        var refresh = _cookieHelper.GetRefreshToken();
        if (string.IsNullOrEmpty(refresh))
            return (false, null);
        var body = new RefreshRequestDto { RefreshToken = refresh };
        var token = await _api.PostUnauthAsync<TokenResponseDto>(ApiEndpoints.AuthRefresh, body, ct);
        if (token == null)
            return (false, null);
        await _cookieHelper.SetAuthCookiesAsync(token, ct);
        return (true, token);
    }

    //Örnek: Access token varligi kontrol edilir; refresh token yerine access token bakilir cunku API her istekte access token kullanir
    public bool IsAuthenticated() => !string.IsNullOrEmpty(_cookieHelper.GetAccessToken());

    public string? GetCurrentUserId()
    {
        var token = _cookieHelper.GetStoredToken();
        return token?.UserId;
    }

    public string? GetCurrentUserName()
    {
        var token = _cookieHelper.GetStoredToken();
        return token?.UserName;
    }

    public List<string> GetCurrentUserRoles()
    {
        var token = _cookieHelper.GetStoredToken();
        return token?.Roles ?? [];
    }
}
