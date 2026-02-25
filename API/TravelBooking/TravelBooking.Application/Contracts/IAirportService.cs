using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Havalimani servisi icin sozlesme (interface)---//
public interface IAirportService
{
    Task<DataResult<Airport>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Airport>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Airport>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<Airport>> GetByIATACodeAsync(string iataCode, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Airport>>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<string>>> GetIataCodesByNameOrIataAsync(string nameOrIata, int maxResults = 10, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Airport airport, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Airport airport, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

