using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Cars;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Cars;

public class CarService : ICarService
{
    private readonly ITravelBookingApiClient _api;

    public CarService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, CarDto? Car)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<CarDto>(ApiEndpoints.CarById(id), ct);
        if (res == null)
            return (false, "Car not found.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<CarDto> Cars)> GetAllAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<PagedResultDto<CarDto>>(ApiEndpoints.CarsPaged(1, 1000), ct);
        if (res == null || res.Data == null)
            return (false, "Could not load cars.", new List<CarDto>());
        var list = res.Data.Items?.ToList() ?? new List<CarDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, PagedResultDto<CarDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var path = ApiEndpoints.CarsPaged(pageNumber, pageSize);
        var res = await _api.GetAsync<PagedResultDto<CarDto>>(path, ct);
        if (res == null)
            return (false, "Araclar yuklenemedi.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<CarDto> Cars)> SearchAsync(string? location, string? category, decimal? maxPricePerDay, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
        if (!string.IsNullOrWhiteSpace(category)) query.Add($"category={Uri.EscapeDataString(category)}");
        if (maxPricePerDay.HasValue) query.Add($"maxPricePerDay={maxPricePerDay}");

        var path = "api/Cars/search?" + string.Join("&", query);
        // Search endpoint IEnumerable donduruyor, PagedResult degil
        var res = await _api.GetAsync<List<CarDto>>(path, ct);
        if (res == null || res.Data == null)
            return (false, "Search could not be performed.", new List<CarDto>());
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateCarDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object>(ApiEndpoints.Cars, dto, ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateCarDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<object>(ApiEndpoints.CarById(id), dto, ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<object>(ApiEndpoints.CarById(id), ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }
}
