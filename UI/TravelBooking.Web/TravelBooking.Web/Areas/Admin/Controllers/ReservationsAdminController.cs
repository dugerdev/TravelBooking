using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReservationsAdminController : Controller
{
    private readonly IAdminService _adminService;

    public ReservationsAdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string? status, string? searchTerm, string? reservationType, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var (success, message, reservations) = await _adminService.GetAllReservationsAsync(status, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            return View(new DTOs.Common.PagedResultDto<DTOs.Reservations.ReservationDto>());
        }

        var filteredReservations = reservations ?? new List<DTOs.Reservations.ReservationDto>();

        // Arama filtresi (PNR veya kullanıcı adı)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            filteredReservations = filteredReservations
                .Where(r => r.PNR.ToLower().Contains(searchLower) || 
                           (r.UserName != null && r.UserName.ToLower().Contains(searchLower)) ||
                           r.AppUserId.ToLower().Contains(searchLower))
                .ToList();
        }

        // Rezervasyon türü filtresi
        if (!string.IsNullOrWhiteSpace(reservationType) && Enum.TryParse<ReservationType>(reservationType, true, out var typeEnum))
        {
            filteredReservations = filteredReservations
                .Where(r => r.ReservationType == typeEnum)
                .ToList();
        }

        // Tarih/saate göre sırala (yeni → eski)
        filteredReservations = filteredReservations
            .OrderByDescending(r => r.CreatedDate)
            .ToList();

        // Pagination
        var totalCount = filteredReservations.Count;
        var pagedItems = filteredReservations
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagedResult = new DTOs.Common.PagedResultDto<DTOs.Reservations.ReservationDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        ViewBag.CurrentStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.ReservationType = reservationType;
        
        return View(pagedResult);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, reservation) = await _adminService.GetReservationByIdAsync(id, ct);
        
        if (!success || reservation == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.CancelReservationAsync(id, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
        }
        else
        {
            TempData["Success"] = "Rezervasyon başarıyla silindi (iptal edildi).";
        }

        return RedirectToAction(nameof(Index));
    }
}
