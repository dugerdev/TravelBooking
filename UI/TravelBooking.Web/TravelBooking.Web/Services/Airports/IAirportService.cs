using TravelBooking.Web.DTOs.Airports;

namespace TravelBooking.Web.Services.Airports;

public interface IAirportService
{
    Task<(bool Success, string Message, List<AirportDto> Airports)> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
}
