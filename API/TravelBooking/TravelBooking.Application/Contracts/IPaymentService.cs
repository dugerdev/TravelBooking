using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

//---Odeme servisi icin sozlesme (interface)---//
public interface IPaymentService
{
    Task<DataResult<Payment>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Payment>>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<Payment>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataResult<PagedResult<Payment>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Result> ProcessPaymentAsync(Guid reservationId, decimal amount, Domain.Enums.PaymentMethod paymentMethod, string transactionId, CancellationToken cancellationToken = default);
}
