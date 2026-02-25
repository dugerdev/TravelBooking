namespace TravelBooking.Application.Dtos;

public class ReservationStatisticsDto
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int CompletedReservations { get; set; }
    public Dictionary<string, int> ReservationsByStatus { get; set; } = [];
    public Dictionary<string, int> ReservationsByMonth { get; set; } = [];
}

public class RevenueStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public decimal ThisYearRevenue { get; set; }
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = [];
}

public class UserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public Dictionary<string, int> UsersByMonth { get; set; } = [];
}
