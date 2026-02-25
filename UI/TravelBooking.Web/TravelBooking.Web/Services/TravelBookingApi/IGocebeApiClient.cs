using TravelBooking.Web.DTOs.Common;

namespace TravelBooking.Web.Services.TravelBookingApi;

public interface ITravelBookingApiClient
{
    Task<ApiResult<T>?> GetAsync<T>(string path, CancellationToken ct = default) where T : class;
    Task<ApiResult<T>?> PostAsync<T>(string path, object? body, CancellationToken ct = default) where T : class;
    Task<ApiResult<T>?> PutAsync<T>(string path, object body, CancellationToken ct = default) where T : class;
    Task<ApiResult<T>?> DeleteAsync<T>(string path, CancellationToken ct = default) where T : class;
    Task<T?> PostUnauthAsync<T>(string path, object body, CancellationToken ct = default) where T : class;
    Task<ApiResult<T>?> PostNoBodyAsync<T>(string path, CancellationToken ct = default) where T : class;
    Task<bool> PostNoContentAsync(string path, object? body, CancellationToken ct = default);
}
