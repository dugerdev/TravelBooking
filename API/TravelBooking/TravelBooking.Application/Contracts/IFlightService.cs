using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Ucus servisi icin sozlesme (interface)---//
public interface IFlightService
{
    Task<DataResult<Flight>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Flight>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Flight>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Flight>>> GetByDepartureAirportAsync(Guid airportId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Flight>>> GetByArrivalAirportAsync(Guid airportId, CancellationToken cancellationToken = default);
    Task<DataResult<Flight>> GetByFlightNumberAsync(string flightNumber, DateTime scheduledDeparture, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Flight>>> SearchFlightsHybridAsync(Guid departureAirportId, Guid arrivalAirportId, DateTime departureDate, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Flight>>> SearchFlightsByIataAndDateAsync(IEnumerable<string> fromIatas, IEnumerable<string> toIatas, DateTime date, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Flight flight, CancellationToken cancellationToken = default);
    Task<Result> AddRangeAsync(IEnumerable<Flight> flights, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Flight flight, CancellationToken cancellationToken = default);
    Task<Result> UpsertAsync(Flight flight, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

