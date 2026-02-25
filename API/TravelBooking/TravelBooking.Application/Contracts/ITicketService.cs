using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Bilet servisi icin sozlesme (interface)---//
public interface ITicketService
{
    Task<DataResult<Ticket>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Ticket>>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Ticket>>> GetByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Ticket>>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Ticket>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Ticket>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<Result> AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<Result> CancelTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Result> AssignSeatAsync(Guid ticketId, string seatNumber, CancellationToken cancellationToken = default);
}
