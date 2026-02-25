using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface IStatisticsService
{
    Task<DataResult<DashboardStatisticsDto>> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default);
    Task<DataResult<ReservationStatisticsDto>> GetReservationStatisticsAsync(CancellationToken cancellationToken = default);
    Task<DataResult<RevenueStatisticsDto>> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<DataResult<UserStatisticsDto>> GetUserStatisticsAsync(CancellationToken cancellationToken = default);
}
