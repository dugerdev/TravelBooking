namespace TravelBooking.Api.Models;

public sealed class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
