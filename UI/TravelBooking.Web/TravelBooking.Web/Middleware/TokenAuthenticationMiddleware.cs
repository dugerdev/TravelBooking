using System.Security.Claims;
using TravelBooking.Web.Helpers;

namespace TravelBooking.Web.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICookieHelper cookieHelper)
    {
        var token = cookieHelper.GetStoredToken();
        if (token != null && !string.IsNullOrEmpty(token.UserId))
        {
            var identity = new ClaimsIdentity("TravelBookingCookie");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, token.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, token.UserName ?? ""));
            var roles = token.Roles ?? [];
            foreach (var r in roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, r));
            context.User = new ClaimsPrincipal(identity);
        }
        await _next(context);
    }
}
