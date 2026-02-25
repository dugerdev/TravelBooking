using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

/// <summary>
/// Car servisi icin sozlesme (interface)
/// </summary>
public interface ICarService
{
    Task<DataResult<Car>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Car>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Car>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Car>>> GetByLocationAsync(string location, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Car>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Car>>> SearchCarsAsync(string? location, string? category, decimal? maxPricePerDay, CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Car>>> SearchCarsWithFiltersAsync(CarSearchFilterDto filters, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Car car, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Car car, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
