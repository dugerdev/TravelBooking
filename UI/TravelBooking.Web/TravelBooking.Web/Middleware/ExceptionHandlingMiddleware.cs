using System.Net;

namespace TravelBooking.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (TaskCanceledException)
        {
            // Istek iptal edildi (timeout, kullanici sayfadan ayrildi, vb.) - beklenen durum.
            logger.LogDebug("Request was canceled (TaskCanceledException)");
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (OperationCanceledException)
        {
            // Istek iptal edildi veya baglanti koptu - beklenen durum.
            logger.LogDebug("Request was canceled (OperationCanceledException)");
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";
            if (!context.Response.HasStarted)
                await context.Response.WriteAsync(
                    "<html><body><h1>Bir hata olustu.</h1><p>Lutfen daha sonra tekrar deneyin.</p></body></html>");
        }
    }
}
