using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos.External;

namespace TravelBooking.Application.Contracts;

public interface IFlightDataSyncService
{
    Task<Result> SyncFlightsAsync(CancellationToken cancellationToken = default);
    Task<Result> SyncFlightsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<DataResult<Guid>> ImportSingleExternalFlightAsync(ExternalFlightDto dto, bool autoCreateAirports, CancellationToken cancellationToken = default);
}
