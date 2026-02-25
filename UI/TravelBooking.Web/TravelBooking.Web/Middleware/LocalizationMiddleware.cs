using System.Globalization;
using TravelBooking.Web.Helpers;
using Microsoft.AspNetCore.Localization;

namespace TravelBooking.Web.Middleware;

public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;

    public LocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICookieHelper cookieHelper)
    {
        var language = cookieHelper.GetLanguage();
        var currency = cookieHelper.GetCurrency();

        // Set the culture using ASP.NET Core's culture provider system
        var culture = new CultureInfo(language);
        
        // Set both CurrentCulture and CurrentUICulture for the current thread
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        
        // Also set it for the HttpContext
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // Set the request culture feature so it's available throughout the request
        var requestCulture = new RequestCulture(culture, culture);
        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(requestCulture, null));

        // Store in Items for easy access in views and controllers
        context.Items["SelectedLanguage"] = language;
        context.Items["SelectedCurrency"] = currency;

        await _next(context);
    }
}
