using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.DTOs.Admin;
using TravelBooking.Web.DTOs.Reservations;
using TravelBooking.Web.Services.Admin;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IAdminService _adminService;

    public DashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var (success, message, stats) = await _adminService.GetDashboardStatisticsAsync(ct);
        if (!success || stats == null)
        {
            ViewBag.StatisticsError = message ?? "İstatistikler yüklenemedi. API bağlantısını ve giriş bilgilerinizi kontrol edin.";
            stats = new DashboardStatisticsDto();
        }

        var (revSuccess, _, revenueStats) = await _adminService.GetRevenueStatisticsAsync(null, null, ct);
        if (revSuccess && revenueStats?.RevenueByMonth != null)
            ViewBag.RevenueByMonth = revenueStats.RevenueByMonth;
        else
            ViewBag.RevenueByMonth = new Dictionary<string, decimal>();

        var (resSuccess, _, reservationStats) = await _adminService.GetReservationStatisticsAsync(ct);
        if (resSuccess && reservationStats != null)
            ViewBag.ReservationStats = reservationStats;
        else
            ViewBag.ReservationStats = new ReservationStatisticsDto();

        var (resvSuccess, _, reservations) = await _adminService.GetAllReservationsAsync(null, ct);
        ViewBag.RecentReservations = (resvSuccess && reservations != null ? reservations.OrderByDescending(r => r.CreatedDate).Take(5).ToList() : new List<ReservationDto>());

        return View(stats);
    }
}
