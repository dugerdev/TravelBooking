using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Admin;

namespace TravelBooking.Web.ViewsComponents;

public class StatisticsViewComponent : ViewComponent
{
    private readonly IAdminService _adminService;

    public StatisticsViewComponent(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get real statistics from API
        var (success, message, stats) = await _adminService.GetDashboardStatisticsAsync(HttpContext.RequestAborted);
        
        var model = new StatisticsViewModel
        {
            TotalFlights = stats?.TotalFlights ?? 0,
            TotalReservations = stats?.TotalReservations ?? 0,
            TotalUsers = stats?.TotalUsers ?? 0,
            TotalRevenue = stats?.TotalRevenue ?? 0
        };

        return View(model);
    }
}

public class StatisticsViewModel
{
    public int TotalFlights { get; set; }
    public int TotalReservations { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
}