using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Airports;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Airports;

public class AirportService : IAirportService
{
    private readonly ITravelBookingApiClient _api;

    public AirportService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, List<AirportDto> Airports)> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        var path = ApiEndpoints.AirportsSearch(query, limit);
        // Search endpoint IEnumerable donduruyor
        var res = await _api.GetAsync<List<AirportDto>>(path, ct);
        if (res == null || res.Data == null)
            return (false, "Havalimani bulunamadi.", new List<AirportDto>());
        return (res.Success, res.Message ?? "", res.Data);
    }
}
