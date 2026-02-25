namespace TravelBooking.Web.DTOs.Auth;

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
