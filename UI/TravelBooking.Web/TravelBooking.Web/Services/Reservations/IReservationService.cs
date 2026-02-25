using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Reservations;

namespace TravelBooking.Web.Services.Reservations;

public interface IReservationService
{
    Task<(bool Success, string Message, Guid? ReservationId)> CreateAsync(CreateReservationDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message, ReservationDto? Reservation)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool Success, string Message, ReservationDto? Reservation)> GetByPNRAsync(string pnr, CancellationToken ct = default);
    Task<(bool Success, string Message, List<ReservationDto> Reservations)> GetMyReservationsAsync(CancellationToken ct = default);
    Task<(bool Success, string Message, PagedResultDto<ReservationDto>? Paged)> GetMyReservationsPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<(bool Success, string Message)> CancelAsync(Guid id, CancellationToken ct = default);
}
