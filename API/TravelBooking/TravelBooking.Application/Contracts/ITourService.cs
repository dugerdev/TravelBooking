using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

/// <summary>
/// Tour servisi icin sozlesme (interface)
/// </summary>
public interface ITourService
{
    Task<DataResult<Tour>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Tour>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Tour>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Tour>>> GetByDestinationAsync(string destination, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Tour>>> SearchToursAsync(string? destination, int? minDuration, int? maxDuration, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Tour tour, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Tour tour, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
