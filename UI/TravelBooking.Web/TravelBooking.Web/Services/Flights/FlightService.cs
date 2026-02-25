using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Airports;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Flights;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Flights;

public class FlightService : IFlightService
{
    private readonly ITravelBookingApiClient _api;

    public FlightService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, List<ExternalFlightDto> Flights)> SearchExternalAsync(string from, string to, DateTime date, int limit = 20, CancellationToken ct = default)
    {
        var path = ApiEndpoints.FlightsSearchExternal(from, to, date, limit);
        var res = await _api.GetAsync<List<ExternalFlightDto>>(path, ct);
        if (res == null)
            return (false, "Search could not be performed.", new List<ExternalFlightDto>());
        var list = res.Data?.ToList() ?? new List<ExternalFlightDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, List<FlightDto> Flights)> SearchHybridAsync(string? from, string? to, DateTime? departureDate, Guid? departureAirportId, Guid? arrivalAirportId, CancellationToken ct = default)
    {
        var q = new List<string>();
        if (!string.IsNullOrWhiteSpace(from)) q.Add($"from={Uri.EscapeDataString(from!)}");
        if (!string.IsNullOrWhiteSpace(to)) q.Add($"to={Uri.EscapeDataString(to!)}");
        if (departureDate.HasValue) q.Add($"departureDate={departureDate.Value:yyyy-MM-dd}");
        if (departureAirportId.HasValue && departureAirportId != Guid.Empty) q.Add($"departureAirportId={departureAirportId}");
        if (arrivalAirportId.HasValue && arrivalAirportId != Guid.Empty) q.Add($"arrivalAirportId={arrivalAirportId}");
        var path = "api/Flights/search?" + string.Join("&", q);
        var res = await _api.GetAsync<List<FlightDto>>(path, ct);
        if (res == null)
            return (false, "Arama yapilamadi.", new List<FlightDto>());
        var list = res.Data?.ToList() ?? new List<FlightDto>();
        return (res.Success, res.Message ?? "", list);
    }

    public async Task<(bool Success, string Message, FlightDto? Flight)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<FlightDto>(ApiEndpoints.FlightById(id), ct);
        if (res == null)
            return (false, "Flight not found.", null);
        return (res.Success, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message, List<AirportDto> Airports)> SearchAirportsAsync(string? query, int limit = 20, CancellationToken ct = default)
    {
        var q = Uri.EscapeDataString(query ?? "");
        var path = ApiEndpoints.AirportsSearch(query ?? "", limit);
        var res = await _api.GetAsync<List<AirportDto>>(path, ct);
        if (res == null)
            return (false, "", new List<AirportDto>());
        var list = res.Data?.ToList() ?? new List<AirportDto>();
        return (true, "", list);
    }
}
