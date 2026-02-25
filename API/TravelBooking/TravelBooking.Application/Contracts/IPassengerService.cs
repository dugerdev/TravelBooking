using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Yolcu servisi icin sozlesme (interface)---//
public interface IPassengerService
{
    Task<DataResult<Passenger>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Passenger>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Passenger>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Passenger passenger, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Passenger passenger, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

