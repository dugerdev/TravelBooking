using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DataResult<DashboardStatisticsDto>> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var usersQuery = _userManager.Users.AsNoTracking();
            var totalUsers = await usersQuery.CountAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var activeUsers = await usersQuery.CountAsync(
                u => !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= now,
                cancellationToken);

            var reservationsQuery = _unitOfWork.Context.Set<Reservation>()
                .AsNoTracking()
                .Where(r => !r.IsDeleted);
            var totalReservations = await reservationsQuery.CountAsync(cancellationToken);
            var pendingReservations = await reservationsQuery.CountAsync(
                r => r.Status == ReservationStatus.Pending,
                cancellationToken);
            var confirmedReservations = await reservationsQuery.CountAsync(
                r => r.Status == ReservationStatus.Confirmed,
                cancellationToken);

            var flightsQuery = _unitOfWork.Context.Set<Flight>()
                .AsNoTracking()
                .Where(f => !f.IsDeleted);
            var totalFlights = await flightsQuery.CountAsync(cancellationToken);
            var activeFlights = await flightsQuery.CountAsync(
                f => f.IsActive && f.AvailableSeats > 0,
                cancellationToken);

            // PaymentStatus enum-to-string conversion causes issues in complex queries
            // Fetch paid reservations and sum in memory to avoid translation errors
            var paidReservations = await reservationsQuery
                .Select(r => new { r.PaymentStatus, r.TotalPrice, r.ReservationDate })
                .ToListAsync(cancellationToken);
            
            var totalRevenue = paidReservations
                .Where(r => r.PaymentStatus == PaymentStatus.Paid)
                .Sum(r => r.TotalPrice);

            var today = DateTime.UtcNow.Date;
            var todayRevenue = paidReservations
                .Where(r => r.PaymentStatus == PaymentStatus.Paid && r.ReservationDate.Date == today)
                .Sum(r => r.TotalPrice);

            var dashboardStats = new DashboardStatisticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalReservations = totalReservations,
                PendingReservations = pendingReservations,
                ConfirmedReservations = confirmedReservations,
                TotalFlights = totalFlights,
                ActiveFlights = activeFlights,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue
            };

            return new SuccessDataResult<DashboardStatisticsDto>(dashboardStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard statistics");
            return new ErrorDataResult<DashboardStatisticsDto>(null!, "Istatistikler alinirken hata olustu.");
        }
    }

    public async Task<DataResult<ReservationStatisticsDto>> GetReservationStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var reservationsQuery = _unitOfWork.Context.Set<Reservation>()
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            var stats = new ReservationStatisticsDto
            {
                TotalReservations = await reservationsQuery.CountAsync(cancellationToken),
                PendingReservations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Pending, cancellationToken),
                ConfirmedReservations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Confirmed, cancellationToken),
                CancelledReservations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Cancelled, cancellationToken),
                CompletedReservations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Completed, cancellationToken)
            };

            var statusGroups = await reservationsQuery
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(cancellationToken);
            stats.ReservationsByStatus = statusGroups.ToDictionary(x => x.Status, x => x.Count);

            var startMonth = DateTime.UtcNow.Date.AddMonths(-11);
            var monthGroups = await reservationsQuery
                .Where(r => r.ReservationDate >= startMonth)
                .GroupBy(r => new { r.ReservationDate.Year, r.ReservationDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var monthLookup = monthGroups.ToDictionary(
                x => $"{x.Year:D4}-{x.Month:D2}",
                x => x.Count);

            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i).ToString("yyyy-MM"))
                .Reverse()
                .ToList();

            stats.ReservationsByMonth = last12Months.ToDictionary(
                month => month,
                month => monthLookup.TryGetValue(month, out var count) ? count : 0);

            return new SuccessDataResult<ReservationStatisticsDto>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation statistics");
            return new ErrorDataResult<ReservationStatisticsDto>(null!, "Rezervasyon istatistikleri alinirken hata olustu.");
        }
    }

    public async Task<DataResult<RevenueStatisticsDto>> GetRevenueStatisticsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservationsQuery = _unitOfWork.Context.Set<Reservation>()
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            if (startDate.HasValue || endDate.HasValue)
            {
                if (startDate.HasValue)
                    reservationsQuery = reservationsQuery.Where(r => r.ReservationDate >= startDate.Value);
                if (endDate.HasValue)
                    reservationsQuery = reservationsQuery.Where(r => r.ReservationDate <= endDate.Value);
            }

            // Fetch all reservations and filter by PaymentStatus in memory to avoid enum-to-string conversion issues
            var allReservations = await reservationsQuery
                .Select(r => new { r.PaymentStatus, r.TotalPrice, r.ReservationDate })
                .ToListAsync(cancellationToken);
            
            var paidReservations = allReservations.Where(r => r.PaymentStatus == PaymentStatus.Paid).ToList();

            var stats = new RevenueStatisticsDto
            {
                TotalRevenue = paidReservations.Sum(r => r.TotalPrice),
                TodayRevenue = paidReservations.Where(r => r.ReservationDate.Date == today).Sum(r => r.TotalPrice),
                ThisMonthRevenue = paidReservations.Where(r => r.ReservationDate >= startOfMonth).Sum(r => r.TotalPrice),
                ThisYearRevenue = paidReservations.Where(r => r.ReservationDate >= startOfYear).Sum(r => r.TotalPrice)
            };

            var startMonth = DateTime.UtcNow.Date.AddMonths(-11);
            var monthGroups = paidReservations
                .Where(r => r.ReservationDate >= startMonth)
                .GroupBy(r => new { r.ReservationDate.Year, r.ReservationDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalPrice) })
                .ToList();

            var monthLookup = monthGroups.ToDictionary(
                x => $"{x.Year:D4}-{x.Month:D2}",
                x => x.Total);

            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i).ToString("yyyy-MM"))
                .Reverse()
                .ToList();

            stats.RevenueByMonth = last12Months.ToDictionary(
                month => month,
                month => monthLookup.TryGetValue(month, out var total) ? total : 0m);

            return new SuccessDataResult<RevenueStatisticsDto>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue statistics");
            return new ErrorDataResult<RevenueStatisticsDto>(null!, "Gelir istatistikleri alinirken hata olustu.");
        }
    }

    public async Task<DataResult<UserStatisticsDto>> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var usersQuery = _userManager.Users.AsNoTracking();
            var now = DateTimeOffset.UtcNow;

            var totalUsers = await usersQuery.CountAsync(cancellationToken);
            var activeUsers = await usersQuery.CountAsync(
                u => !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= now,
                cancellationToken);
            var lockedUsers = await usersQuery.CountAsync(
                u => u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > now,
                cancellationToken);

            var stats = new UserStatisticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                LockedUsers = lockedUsers
            };

            var startMonth = DateTime.UtcNow.Date.AddMonths(-11);
            var monthGroups = await usersQuery
                .Where(u => u.CreatedDate >= startMonth)
                .GroupBy(u => new { u.CreatedDate.Year, u.CreatedDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var monthLookup = monthGroups.ToDictionary(
                x => $"{x.Year:D4}-{x.Month:D2}",
                x => x.Count);

            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i).ToString("yyyy-MM"))
                .Reverse()
                .ToList();

            stats.UsersByMonth = last12Months.ToDictionary(
                month => month,
                month => monthLookup.TryGetValue(month, out var count) ? count : 0);

            return new SuccessDataResult<UserStatisticsDto>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return new ErrorDataResult<UserStatisticsDto>(null!, "Kullanici istatistikleri alinirken hata olustu.");
        }
    }
}
