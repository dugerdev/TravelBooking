namespace TravelBooking.Web.DTOs.Admin;

public class RevenueStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public decimal ThisYearRevenue { get; set; }
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}
