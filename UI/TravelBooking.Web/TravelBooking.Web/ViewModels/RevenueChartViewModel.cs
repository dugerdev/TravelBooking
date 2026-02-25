namespace TravelBooking.Web.ViewModels;

public class RevenueChartViewModel
{
    public Dictionary<string, decimal> Data { get; set; } = new();
    public string Title { get; set; } = "Gelir Özeti";
    public string CanvasId { get; set; } = "revenueChart";
}
