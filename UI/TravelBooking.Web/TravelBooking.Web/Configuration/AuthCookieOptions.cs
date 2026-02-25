namespace TravelBooking.Web.Configuration;

public class AuthCookieOptions
{
    public const string SectionName = "Authentication";
    public string CookieName { get; set; } = "GocebAuth";
    public int CookieExpireMinutes { get; set; } = 120;
    public string RefreshTokenCookieName { get; set; } = "GocebRefresh";
}
