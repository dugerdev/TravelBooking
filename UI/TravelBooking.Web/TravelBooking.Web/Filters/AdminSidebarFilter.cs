using TravelBooking.Web.Configuration;
using TravelBooking.Web.Helpers;
using TravelBooking.Web.Services;
using TravelBooking.Web.Services.ContactMessages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace TravelBooking.Web.Filters;

/// <summary>
/// Admin sayfalarinda sidebar icin bekleyen yorum, okunmamis ileti sayilari ve Swagger URL ekler.
/// </summary>
public class AdminSidebarFilter : IAsyncActionFilter
{
    private readonly TestimonialService _testimonialService;
    private readonly IContactMessageService _contactMessageService;
    private readonly ICookieHelper _cookieHelper;
    private readonly string _swaggerUrl;

    public AdminSidebarFilter(
        TestimonialService testimonialService,
        IContactMessageService contactMessageService,
        ICookieHelper cookieHelper,
        IOptions<TravelBookingApiOptions> apiOptions)
    {
        _testimonialService = testimonialService;
        _contactMessageService = contactMessageService;
        _cookieHelper = cookieHelper;
        var baseUrl = apiOptions?.Value?.BaseUrl?.TrimEnd('/') ?? "https://localhost:7283";
        _swaggerUrl = baseUrl + "/swagger";
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values["area"]?.ToString()?.Equals("Admin", StringComparison.OrdinalIgnoreCase) != true)
        {
            await next();
            return;
        }

        try
        {
            var token = _cookieHelper.GetAccessToken();
            int pendingCount = 0;
            if (!string.IsNullOrEmpty(token))
            {
                var pendingResult = await _testimonialService.GetPendingTestimonialsAsync(token);
                pendingCount = pendingResult.Success && pendingResult.Data != null ? pendingResult.Data.Count : 0;
            }
            var unreadCount = await _contactMessageService.GetUnreadCountAsync(context.HttpContext.RequestAborted);

            context.HttpContext.Items["PendingTestimonialsCount"] = pendingCount;
            context.HttpContext.Items["UnreadMessagesCount"] = unreadCount;
            context.HttpContext.Items["SwaggerUrl"] = _swaggerUrl;
        }
        catch
        {
            context.HttpContext.Items["PendingTestimonialsCount"] = 0;
            context.HttpContext.Items["UnreadMessagesCount"] = 0;
            context.HttpContext.Items["SwaggerUrl"] = _swaggerUrl;
        }

        await next();
    }
}
