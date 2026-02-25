using TravelBooking.Application.Dtos.External;

namespace TravelBooking.Application.Abstractions.External;

public interface IExternalFlightApiClient
{
    Task<IEnumerable<ExternalFlightDto>> GetFlightsAsync(CancellationToken cancellationToken = default);
    /// <summary>Havalimanindan havalimanina, tarih ile Aviationstack'ten ucus arar. dep_iata, arr_iata, flight_date API'ye gonderilir.</summary>
    Task<IEnumerable<ExternalFlightDto>> GetFlightsFilteredAsync(string depIata, string arrIata, DateTime flightDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExternalFlightDto>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

