using System.Text.Json;
using TravelBooking.Web.Configuration;
using TravelBooking.Web.DTOs.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TravelBooking.Web.Helpers;

public interface ICookieHelper
{
    Task SetAuthCookiesAsync(TokenResponseDto token, CancellationToken ct = default);
    string? GetAccessToken();
    string? GetRefreshToken();
    void ClearAuthCookies();
    TokenResponseDto? GetStoredToken();
    void SetCurrency(string currency);
    string GetCurrency();
    void SetLanguage(string language);
    string GetLanguage();
}

public class CookieHelper(IHttpContextAccessor httpContextAccessor, IOptions<AuthCookieOptions> options, IWebHostEnvironment env) : ICookieHelper
{
    private readonly AuthCookieOptions authOptions = options.Value;
    private readonly bool isDevelopment = env.IsDevelopment();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task SetAuthCookiesAsync(TokenResponseDto token, CancellationToken ct = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return Task.CompletedTask;

        var json = JsonSerializer.Serialize(token, JsonOptions);
        var cookieOpts = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromMinutes(authOptions.CookieExpireMinutes)
        };

        context.Response.Cookies.Append(authOptions.CookieName, json, cookieOpts);

        var refreshOpts = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(14)
        };
        context.Response.Cookies.Append(authOptions.RefreshTokenCookieName, token.RefreshToken, refreshOpts);

        return Task.CompletedTask;
    }

    public string? GetAccessToken()
    {
        var t = GetStoredToken();
        return t?.AccessToken;
    }

    public string? GetRefreshToken()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Cookies.TryGetValue(authOptions.RefreshTokenCookieName, out var v) == true)
            return v;
        var t = GetStoredToken();
        return t?.RefreshToken;
    }

    public void ClearAuthCookies()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return;
        context.Response.Cookies.Delete(authOptions.CookieName, new CookieOptions { Path = "/" });
        context.Response.Cookies.Delete(authOptions.RefreshTokenCookieName, new CookieOptions { Path = "/" });
    }

    public TokenResponseDto? GetStoredToken()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Cookies.TryGetValue(authOptions.CookieName, out var json) != true || string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<TokenResponseDto>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public void SetCurrency(string currency)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return;

        var cookieOpts = new CookieOptions
        {
            HttpOnly = false,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(365)
        };

        context.Response.Cookies.Append("TravelBookingCurrency", currency, cookieOpts);
    }

    public string GetCurrency()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Cookies.TryGetValue("TravelBookingCurrency", out var currency) == true && !string.IsNullOrEmpty(currency))
            return currency;
        return "TRY";
    }

    public void SetLanguage(string language)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return;

        var cookieOpts = new CookieOptions
        {
            HttpOnly = false,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
            MaxAge = TimeSpan.FromDays(365)
        };

        context.Response.Cookies.Append("TravelBookingLanguage", language, cookieOpts);

        // Set ASP.NET Core culture cookie so RequestLocalization middleware picks it up
        var requestCulture = new RequestCulture(language);
        var cultureCookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);
        context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, cultureCookieValue, cookieOpts);
    }

    public string GetLanguage()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Cookies.TryGetValue("TravelBookingLanguage", out var language) == true && !string.IsNullOrEmpty(language))
            return language;
        return "tr";
    }
}
