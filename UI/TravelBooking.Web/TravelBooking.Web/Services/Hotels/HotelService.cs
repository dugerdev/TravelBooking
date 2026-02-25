using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Hotels;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Hotels;

public class HotelService : IHotelService
{
    private readonly ITravelBookingApiClient _api;

    public HotelService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, HotelDto? Hotel)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<HotelDto>(ApiEndpoints.HotelById(id), ct);
        if (res == null)
            return (false, "Hotel not found.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<HotelDto> Hotels)> GetAllAsync(CancellationToken ct = default)
    {
        // API PagedResult donduruyor, tum kayitlari almak icin buyuk bir PageSize kullaniyoruz
        var res = await _api.GetAsync<PagedResultDto<HotelDto>>(ApiEndpoints.HotelsPaged(1, 1000), ct);
        if (res == null || res.Data == null)
            return (false, "Could not load hotels.", new List<HotelDto>());
        var list = res.Data.Items?.ToList() ?? new List<HotelDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, PagedResultDto<HotelDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var path = ApiEndpoints.HotelsPaged(pageNumber, pageSize);
        var res = await _api.GetAsync<PagedResultDto<HotelDto>>(path, ct);
        if (res == null)
            return (false, "Could not load hotels.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<HotelDto> Hotels)> SearchAsync(string? city, int? minStarRating, decimal? maxPricePerNight, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(city)) query.Add($"city={Uri.EscapeDataString(city)}");
        if (minStarRating.HasValue) query.Add($"minStarRating={minStarRating}");
        if (maxPricePerNight.HasValue) query.Add($"maxPricePerNight={maxPricePerNight}");

        var path = ApiEndpoints.HotelsSearch(string.Join("&", query));
        // Search endpoint IEnumerable donduruyor, PagedResult degil
        var res = await _api.GetAsync<List<HotelDto>>(path, ct);
        if (res == null || res.Data == null)
            return (false, "Search could not be performed.", new List<HotelDto>());
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateHotelDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object>(ApiEndpoints.Hotels, dto, ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateHotelDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<object>(ApiEndpoints.HotelById(id), dto, ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<object>(ApiEndpoints.HotelById(id), ct);
        if (res == null) return (false, "Server did not respond.");
        return (res.Success, res.Message ?? "");
    }
}
