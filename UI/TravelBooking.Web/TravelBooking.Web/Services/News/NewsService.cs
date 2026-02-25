using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.News;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.News;

public class NewsService : INewsService
{
    private readonly ITravelBookingApiClient _api;

    public NewsService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, NewsDto? News)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<NewsDto>(ApiEndpoints.NewsById(id), ct);
        if (res == null)
            return (false, "Haber bulunamadi.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<NewsDto> News)> GetAllAsync(CancellationToken ct = default)
    {
        // API pagination olmadan IEnumerable donduruyor, bu yuzden PagedResult ile cekiyoruz
        var res = await _api.GetAsync<PagedResultDto<NewsDto>>(ApiEndpoints.NewsPaged(1, 1000), ct);
        if (res == null || res.Data == null)
            return (false, "Haberler yuklenemedi.", new List<NewsDto>());
        var list = res.Data.Items?.ToList() ?? new List<NewsDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, PagedResultDto<NewsDto>? Paged)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var path = ApiEndpoints.NewsPaged(pageNumber, pageSize);
        var res = await _api.GetAsync<PagedResultDto<NewsDto>>(path, ct);
        if (res == null)
            return (false, "Haberler yuklenemedi.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<NewsDto> News)> GetPublishedAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<PagedResultDto<NewsDto>>(ApiEndpoints.NewsPaged(1, 1000), ct);
        if (res == null || res.Data == null)
            return (false, "Haberler yuklenemedi.", new List<NewsDto>());
        var list = res.Data.Items?.ToList() ?? new List<NewsDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, List<NewsDto> News)> SearchAsync(string? query, string? category, CancellationToken ct = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(query)) queryParams.Add($"query={Uri.EscapeDataString(query)}");
        if (!string.IsNullOrWhiteSpace(category)) queryParams.Add($"category={Uri.EscapeDataString(category)}");

        var path = ApiEndpoints.NewsSearch(string.Join("&", queryParams));
        // Search endpoint IEnumerable donduruyor, PagedResult degil
        var res = await _api.GetAsync<List<NewsDto>>(path, ct);
        if (res == null || res.Data == null)
            return (false, "Arama yapilamadi.", new List<NewsDto>());
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateNewsDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<NewsDto>(ApiEndpoints.News, dto, ct);
        if (res == null)
            return (false, "Haber olusturulamadi.");
        return (res.Success, res.Message ?? "Haber basariyla olusturuldu.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateNewsDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<NewsDto>(ApiEndpoints.NewsById(id), dto, ct);
        if (res == null)
            return (false, "Haber guncellenemedi.");
        return (res.Success, res.Message ?? "Haber basariyla guncellendi.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<NewsDto>(ApiEndpoints.NewsById(id), ct);
        if (res == null)
            return (false, "Haber silinemedi.");
        return (res.Success, res.Message ?? "Haber basariyla silindi.");
    }
}
