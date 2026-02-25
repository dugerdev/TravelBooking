using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Rezervasyon servisi icin sozlesme (interface)---//
public interface IReservationService
{
    Task<DataResult<Reservation>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<Reservation>> GetByPNRAsync(string pnr, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Reservation>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Reservation>>> GetByUserIdPagedAsync(string userId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Reservation>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Reservation>>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Reservation>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<DataResult<Guid>> CreateReservationWithTicketsAndPaymentAsync(CreateReservationDto dto, CancellationToken cancellationToken = default);
}

