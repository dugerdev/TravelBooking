using System.Text;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Api.Middleware;

//Örnek: Request/response loglama - hassas veriler (sifre, token) regex ile maskeleyerek loglar
public class RequestResponseLoggingMiddleware
{
    //Örnek: Loglama oncesi maskelenecek alanlar (password, token, secret vb.)
    private static readonly (string Pattern, string Replacement)[] SensitiveDataPatterns = new[]
    {
        (@"(""password""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""Password""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""token""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""Token""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""secret""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""Secret""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""accessToken""\s*:\s*"")[^""]*("")", "$1****$2"),
        (@"(""refreshToken""\s*:\s*"")[^""]*("")", "$1****$2")
    };
    
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly bool _enableLogging;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _enableLogging = configuration.GetValue<bool>("Logging:EnableRequestResponseLogging", false);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_enableLogging)
        {
            await _next(context);
            return;
        }

        //---Request bilgilerini logla---//
        await LogRequestAsync(context);

        //Örnek: Response stream'i MemoryStream'e yonlendirir; log sonrasi orijinal stream'e geri yazar
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            //Örnek: Response body loglanir ve client'a gonderilir
            await LogResponseAsync(context, responseBody, originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request: {Path}", context.Request.Path);
            throw;
        }
        //Örnek: Her durumda response body orijinal stream'e kopyalanir (client'a gonderilir)
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        try
        {
            var request = context.Request;
            
            //---Sensitive endpoint'leri loglamadan haric tut---//
            if (request.Path.StartsWithSegments("/api/auth") || 
                request.Path.StartsWithSegments("/health"))
            {
                return;
            }

            _logger.LogInformation(
                "Request: {Method} {Path} | Query: {QueryString} | IP: {RemoteIpAddress}",
                request.Method,
                request.Path,
                request.QueryString,
                context.Connection.RemoteIpAddress);

            //---Request body'yi logla (sadece POST, PUT, PATCH icin)---//
            if (request.ContentLength > 0 &&
                (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH"))
            {
                request.EnableBuffering();
                using var reader = new StreamReader(
                    request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);
                var bodyAsText = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                //---Sensitive data'yi maskele (password, token vb.)---//
                bodyAsText = MaskSensitiveData(bodyAsText);

                _logger.LogDebug("Request Body: {Body}", bodyAsText);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request");
        }
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
    {
        try
        {
            var request = context.Request;
            
            //---Sensitive endpoint'leri loglamadan haric tut---//
            if (request.Path.StartsWithSegments("/api/auth") || 
                request.Path.StartsWithSegments("/health"))
            {
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation(
                "Response: {StatusCode} | Path: {Path} | Size: {Size} bytes",
                context.Response.StatusCode,
                request.Path,
                responseBody.Length);

            //---Response body'yi logla (sadece hata durumlarinda veya debug modunda)---//
            if (context.Response.StatusCode >= 400 || _logger.IsEnabled(LogLevel.Debug))
            {
                var maskedBody = MaskSensitiveData(responseBodyText);
                _logger.LogDebug("Response Body: {Body}", maskedBody);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging response");
        }
    }

    //Örnek: Regex ile password, token, secret gibi alanlarin degerlerini **** ile degistirir
    private string MaskSensitiveData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return data;

        //---Password, token, secret gibi sensitive field'lari maskele---//
        var result = data;
        foreach (var (pattern, replacement) in SensitiveDataPatterns)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return result;
    }
}
