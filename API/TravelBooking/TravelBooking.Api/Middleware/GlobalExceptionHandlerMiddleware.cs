using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Api.Middleware;

/// <summary>
/// Global exception handling middleware. Catches all unhandled exceptions and returns
/// consistent JSON error responses. Handles ValidationException, UnauthorizedAccessException,
/// timeout/cancellation, and database errors.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {

        // Iptal/zaman asimi beklenen durumdur; diger hatalari Error olarak logla
        var isCancelOrTimeout = ex is TaskCanceledException or OperationCanceledException;
        if (isCancelOrTimeout)
            _logger.LogInformation(ex, "Request canceled or timed out. Path: {Path}, Method: {Method}", context.Request.Path, context.Request.Method);
        else
            _logger.LogError(ex, "Unhandled exception occurred. Path: {Path}, Method: {Method}, TraceId: {TraceId}", context.Request.Path, context.Request.Method, context.TraceIdentifier);

        var status = ex switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            TaskCanceledException => StatusCodes.Status504GatewayTimeout,
            OperationCanceledException => StatusCodes.Status408RequestTimeout,
            DbUpdateException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = status switch
        {
            StatusCodes.Status400BadRequest => "Gecersiz Istek",
            StatusCodes.Status401Unauthorized => "Yetkisiz Erisim",
            StatusCodes.Status408RequestTimeout => "Istek iptal edildi",
            StatusCodes.Status504GatewayTimeout => "Istek zaman asimina ugradi",
            _ => "Beklenmeyen bir hata olustu."
        };

        Exception? innerException = ex switch
        {
            DbUpdateException dbEx => dbEx.InnerException,
            TaskCanceledException tcEx when tcEx.InnerException != null => tcEx.InnerException,
            OperationCanceledException ocEx when ocEx.InnerException != null => ocEx.InnerException,
            AggregateException aggEx => aggEx.InnerException,
            _ => null
        };

        string? timeoutDetails = null;
        if (ex is TaskCanceledException taskCanceledEx)
        {
            var methodHint = GetMethodHint(taskCanceledEx.StackTrace);
            timeoutDetails = _environment.IsDevelopment()
                ? $"TaskCanceledException: {taskCanceledEx.Message}. Operation Type: {methodHint}."
                : "Istek zaman asimina ugradi. Lutfen daha sonra tekrar deneyin.";
            _logger.LogInformation("TaskCanceledException. Path: {Path}, Method: {Method}, OperationType: {OperationType}",
                context.Request.Path, context.Request.Method, methodHint);
        }
        else if (ex is OperationCanceledException opCancelEx)
        {
            var methodHint = GetMethodHint(opCancelEx.StackTrace);
            timeoutDetails = _environment.IsDevelopment()
                ? $"OperationCanceledException: {opCancelEx.Message}. Operation Type: {methodHint}."
                : "Istek iptal edildi veya baglanti koptu. Lutfen tekrar deneyin.";
            _logger.LogInformation("OperationCanceledException. Path: {Path}, Method: {Method}, OperationType: {OperationType}",
                context.Request.Path, context.Request.Method, methodHint);
        }

        string? errorDetail = _environment.IsDevelopment()
            ? (timeoutDetails ?? (innerException != null ? $"{ex.Message} | Inner: {innerException.Message}" : ex.Message))
            : timeoutDetails;

        object? validationErrors = null;
        if (ex is ValidationException validationEx)
        {
            validationErrors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var response = new
        {
            Title = title,
            Status = status,
            Detail = errorDetail,
            Errors = validationErrors,
            InnerException = _environment.IsDevelopment() && innerException != null ? innerException.Message : null,
            Path = _environment.IsDevelopment() ? context.Request.Path.ToString() : null,
            Method = _environment.IsDevelopment() ? context.Request.Method : null,
            TraceId = context.TraceIdentifier
        };

        if (!context.Response.HasStarted)
            await context.Response.WriteAsJsonAsync(response);
    }

    private static string GetMethodHint(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace)) return "Unknown";
        if (stackTrace.Contains("GetFlightsAsync") || stackTrace.Contains("ExternalFlightApiClient")) return "External API Call";
        if (stackTrace.Contains("SaveChangesAsync") || stackTrace.Contains("DbContext")) return "Database Operation";
        if (stackTrace.Contains("HttpClient")) return "HTTP Request";
        return "Unknown";
    }
}
