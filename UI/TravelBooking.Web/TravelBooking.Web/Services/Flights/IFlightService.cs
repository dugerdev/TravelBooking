using TravelBooking.Web.DTOs.Airports;
using TravelBooking.Web.DTOs.Flights;

namespace TravelBooking.Web.Services.Flights;

public interface IFlightService
{
    Task<(bool Success, string Message, List<ExternalFlightDto> Flights)> SearchExternalAsync(string from, string to, DateTime date, int limit = 20, CancellationToken ct = default);
    Task<(bool Success, string Message, List<FlightDto> Flights)> SearchHybridAsync(string? from, string? to, DateTime? departureDate, Guid? departureAirportId, Guid? arrivalAirportId, CancellationToken ct = default);
    Task<(bool Success, string Message, FlightDto? Flight)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, List<AirportDto> Airports)> SearchAirportsAsync(string? query, int limit = 20, CancellationToken ct = default);
}
