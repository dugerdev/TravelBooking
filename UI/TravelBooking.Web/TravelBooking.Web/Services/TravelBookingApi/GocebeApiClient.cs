using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TravelBooking.Web.DTOs.Common;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Polly.CircuitBreaker;

namespace TravelBooking.Web.Services.TravelBookingApi;

public class TravelBookingApiClient(HttpClient httpClient, IOptions<Configuration.TravelBookingApiOptions> options) : ITravelBookingApiClient
{
    private readonly HttpClient _httpClient = ConfigureHttpClient(httpClient, options);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true) }
    };

    //Örnek: API base URL, timeout ve Accept header ayarlarini yapar
    private static HttpClient ConfigureHttpClient(HttpClient client, IOptions<Configuration.TravelBookingApiOptions> opts)
    {
        var baseUrl = opts.Value.BaseUrl.TrimEnd('/');
        client.BaseAddress = new Uri(baseUrl + "/");
        client.Timeout = TimeSpan.FromSeconds(opts.Value.TimeoutSeconds);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        return client;
    }

    //Örnek: Kimlik dogrulama gerektiren GET istekleri icin; CircuitBreaker aciksa veya baglanti hatasi varsa kullaniciya mesaj doner
    public async Task<ApiResult<T>?> GetAsync<T>(string path, CancellationToken ct = default) where T : class
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            var response = await _httpClient.GetAsync(pathTrimmed, ct);
            return await ParseResultAsync<T>(response, ct);
        }
        //Örnek: Kullanici islemi iptal ettiyse null doner
        catch (OperationCanceledException)
        {
            return null;
        }
        //Örnek: Circuit breaker acik - API sik basarisiz olunca devreye girer, bir sure istek gonderilmez
        catch (BrokenCircuitException)
        {
            return new ApiResult<T> { Success = false, Message = "API is temporarily unavailable. Please refresh the page in a few seconds.", Data = default };
        }
        //Örnek: Ag veya sunucu hatasi - API ulasilamaz
        catch (HttpRequestException)
        {
            return new ApiResult<T> { Success = false, Message = "API'ye baglanilamadi. API'nin calistigindan emin olun.", Data = default };
        }
    }

    public async Task<ApiResult<T>?> PostAsync<T>(string path, object? body, CancellationToken ct = default) where T : class
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            var content = body != null ? new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json") : null;
            var response = content != null
                ? await _httpClient.PostAsync(pathTrimmed, content, ct)
                : await _httpClient.PostAsync(pathTrimmed, null, ct);
            return await ParseResultAsync<T>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (BrokenCircuitException)
        {
            return new ApiResult<T> { Success = false, Message = "API is temporarily unavailable. Please refresh the page in a few seconds.", Data = default };
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T> { Success = false, Message = "API'ye baglanilamadi. API'nin calistigindan emin olun.", Data = default };
        }
    }

    public async Task<ApiResult<T>?> PutAsync<T>(string path, object body, CancellationToken ct = default) where T : class
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(pathTrimmed, content, ct);
            return await ParseResultAsync<T>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (BrokenCircuitException)
        {
            return new ApiResult<T> { Success = false, Message = "API is temporarily unavailable. Please refresh the page in a few seconds.", Data = default };
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T> { Success = false, Message = "API'ye baglanilamadi. API'nin calistigindan emin olun.", Data = default };
        }
    }

    public async Task<ApiResult<T>?> DeleteAsync<T>(string path, CancellationToken ct = default) where T : class
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            var response = await _httpClient.DeleteAsync(pathTrimmed, ct);
            return await ParseResultAsync<T>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (BrokenCircuitException)
        {
            return new ApiResult<T> { Success = false, Message = "API is temporarily unavailable. Please refresh the page in a few seconds.", Data = default };
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T> { Success = false, Message = "API'ye baglanilamadi. API'nin calistigindan emin olun.", Data = default };
        }
    }

    public async Task<ApiResult<T>?> PostNoBodyAsync<T>(string path, CancellationToken ct = default) where T : class
        => await PostAsync<T>(path, null, ct);

    //Örnek: Login, kayit gibi kimlik dogrulama gerektirmeyen POST istekleri; hata durumunda API'nin Message/Errors alanlarini parse eder
    public async Task<T?> PostUnauthAsync<T>(string path, object body, CancellationToken ct = default) where T : class
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(pathTrimmed, content, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                //Örnek: Hata durumunda response body'deki Message veya Errors alanini parse edip kullaniciya anlamli mesaj verir
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        // API'den gelen hata mesajini parse etmeye calis
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
                        if (errorObj != null && errorObj.TryGetValue("Message", out var msgObj))
                            throw new HttpRequestException(TranslateErrorMessage(msgObj?.ToString() ?? ""));
                        if (errorObj != null && errorObj.TryGetValue("Errors", out var errors) && errors != null)
                        {
                            var errorMessages = ParseAndTranslateErrors(errors);
                            throw new HttpRequestException(errorMessages);
                        }
                    }
                    catch (JsonException)
                    {
                        // JSON parse edilemezse raw mesaji kullan
                    }
                }
                throw new HttpRequestException("Request failed. Please try again.");
            }

            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (OperationCanceledException)
        {
            return default;
        }
    }

    //Örnek: API'den gelen Errors objesini (dizi veya string) parse edip her birini TranslateErrorMessage ile cevirir
    private static string ParseAndTranslateErrors(object errors)
    {
        try
        {
            List<string> errorList = [];
            
            //Örnek: FluentValidation Errors genellikle JsonElement dizi veya string olarak gelir
            if (errors is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        var errorMsg = item.GetString();
                        if (!string.IsNullOrWhiteSpace(errorMsg))
                            errorList.Add(TranslateErrorMessage(errorMsg));
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    var errorMsg = jsonElement.GetString();
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                        errorList.Add(TranslateErrorMessage(errorMsg));
                }
            }
            else
            {
                //Örnek: JsonElement degilse string olarak al
                var errorStr = errors.ToString();
                if (!string.IsNullOrWhiteSpace(errorStr))
                    errorList.Add(TranslateErrorMessage(errorStr));
            }
            
            return errorList.Count > 0 ? string.Join(" ", errorList) : "Bir hata olustu.";
        }
        catch
        {
            return "An error occurred.";
        }
    }

    //Örnek: API'nin Turkce/Ingilizce hata mesajlarini kullaniciya uygun sekilde cevirir (sifre kurallari, kullanici adi/email hatalari)
    private static string TranslateErrorMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "";

        //Örnek: Sifre kurallari - kullanicinin anlayacagi mesajlara cevir
        if (message.Contains("Passwords must be at least") && message.Contains("characters"))
            return "Password must be at least 8 characters.";
        if (message.Contains("Passwords must have at least one non alphanumeric character"))
            return "Password must contain at least one special character (!@#$%^&* etc.).";
        if (message.Contains("Passwords must have at least one lowercase"))
            return "Password must contain at least one lowercase letter (a-z).";
        if (message.Contains("Passwords must have at least one uppercase"))
            return "Password must contain at least one uppercase letter (A-Z).";
        if (message.Contains("Passwords must have at least one digit"))
            return "Password must contain at least one digit (0-9).";

        //Örnek: Kullanici adi ve email hatalari
        if (message.Contains("Username") && message.Contains("already taken"))
            return "This username is already in use.";
        if (message.Contains("Email") && message.Contains("already taken"))
            return "This email address is already in use.";
        if (message.Contains("Invalid email"))
            return "Invalid email address.";
        if (message.Contains("Invalid username"))
            return "Invalid username.";

        //Örnek: Giris hatalari
        if (message.Contains("Invalid credentials") || message.Contains("Invalid login"))
            return "Invalid email/username or password.";
        if (message.Contains("Account locked"))
            return "Your account has been locked. Please contact support.";

        //Örnek: Diger tum mesajlar oldugu gibi birakilir
        return message;
    }

    public async Task<bool> PostNoContentAsync(string path, object? body, CancellationToken ct = default)
    {
        try
        {
            var pathTrimmed = path.TrimStart('/');
            HttpContent? content = null;
            if (body != null)
                content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
            var response = content != null
                ? await _httpClient.PostAsync(pathTrimmed, content, ct)
                : await _httpClient.PostAsync(pathTrimmed, null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task<ApiResult<T>?> ParseResultAsync<T>(HttpResponseMessage response, CancellationToken ct) where T : class
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
            return new ApiResult<T> { Success = false, Message = "Empty response.", Data = default };
        var raw = JsonSerializer.Deserialize<ApiResult<T>>(json, _jsonOptions);
        return raw;
    }
}
