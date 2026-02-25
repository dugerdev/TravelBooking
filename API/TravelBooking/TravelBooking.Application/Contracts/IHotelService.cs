using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

/// <summary>
/// Hotel servisi icin sozlesme (interface)
/// </summary>
public interface IHotelService
{
    Task<DataResult<Hotel>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Hotel>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Hotel>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Hotel>>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Hotel>>> SearchHotelsAsync(string? city, int? minStarRating, decimal? maxPricePerNight, CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Hotel>>> SearchHotelsWithFiltersAsync(HotelSearchFilterDto filters, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void ClearCache();
}
