using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Tours;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Tours;

public class TourService : ITourService
{
    private readonly ITravelBookingApiClient _api;

    public TourService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, TourDto? Tour)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<TourDto>(ApiEndpoints.TourById(id), ct);
        if (res == null)
            return (false, "Tur bulunamadi.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<TourDto> Tours)> GetAllAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<PagedResultDto<TourDto>>("api/Tours?PageNumber=1&PageSize=1000", ct);
        if (res == null || res.Data == null)
            return (false, "Turlar yuklenemedi.", new List<TourDto>());
        var list = res.Data.Items?.ToList() ?? new List<TourDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, PagedResultDto<TourDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var path = ApiEndpoints.ToursPaged(pageNumber, pageSize);
        var res = await _api.GetAsync<PagedResultDto<TourDto>>(path, ct);
        if (res == null)
            return (false, "Turlar yuklenemedi.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<TourDto> Tours)> SearchAsync(string? destination, int? minDuration, int? maxDuration, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(destination)) query.Add($"destination={Uri.EscapeDataString(destination)}");
        if (minDuration.HasValue) query.Add($"minDuration={minDuration}");
        if (maxDuration.HasValue) query.Add($"maxDuration={maxDuration}");

        var path = ApiEndpoints.ToursSearch(string.Join("&", query));
        var res = await _api.GetAsync<List<TourDto>>(path, ct);
        if (res == null || res.Data == null)
            return (false, "Arama yapilamadi.", new List<TourDto>());
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateTourDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object>(ApiEndpoints.Tours, dto, ct);
        if (res == null)
            return (false, "Tur eklenemedi.");
        return res.Success ? (true, res.Message ?? "Tur eklendi.") : (false, res.Message ?? "Ekleme basarisiz.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateTourDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<object>(ApiEndpoints.TourById(id), dto, ct);
        if (res == null)
            return (false, "Tur guncellenemedi.");
        return res.Success ? (true, res.Message ?? "Tur guncellendi.") : (false, res.Message ?? "Guncelleme basarisiz.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<object>(ApiEndpoints.TourById(id), ct);
        if (res == null)
            return (false, "Tur silinemedi.");
        return res.Success ? (true, res.Message ?? "Tur silindi.") : (false, res.Message ?? "Silme basarisiz.");
    }
}
