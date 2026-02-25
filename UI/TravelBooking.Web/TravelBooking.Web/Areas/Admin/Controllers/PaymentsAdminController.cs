using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelBooking.Web.Services.Admin;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PaymentsController : Controller
{
    private readonly IAdminService _adminService;

    public PaymentsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate, 
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        // Get ALL reservations (admin view)
        var (success, message, reservations) = await _adminService.GetAllReservationsAsync(status, ct);
        
        // Convert reservations to payment display format
        var payments = reservations?
            .Select(r => new PaymentDisplayDto
            {
                Id = r.Id,
                ReservationId = r.Id,
                PNR = r.PNR,
                TransactionId = $"TXN-{r.Id.ToString().Substring(0, 8)}",
                TransactionAmount = r.TotalPrice,
                Currency = r.Currency.ToString(),
                PaymentMethod = r.PaymentMethod.ToString(),
                TransactionType = "Payment",
                CreatedAt = r.CreatedDate,
                PaymentStatus = r.PaymentStatus.ToString()
            })
            .ToList() ?? new List<PaymentDisplayDto>();
        
        // Apply date filters
        if (startDate.HasValue)
        {
            payments = payments.Where(p => p.CreatedAt >= startDate.Value).ToList();
        }
        
        if (endDate.HasValue)
        {
            payments = payments.Where(p => p.CreatedAt <= endDate.Value).ToList();
        }
        
        // Apply status filter
        if (!string.IsNullOrEmpty(status))
        {
            payments = payments.Where(p => p.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        ViewBag.Status = status;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.CurrentPage = page;
        ViewBag.Message = message;
        
        return View(payments);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        // Get reservation by ID (admin can access any reservation)
        var (success, message, reservation) = await _adminService.GetReservationByIdAsync(id, ct);
        
        if (!success || reservation == null)
        {
            TempData["ErrorMessage"] = message ?? "Payment not found.";
            return RedirectToAction(nameof(Index));
        }
        
        var paymentDisplay = new PaymentDisplayDto
        {
            Id = reservation.Id,
            ReservationId = reservation.Id,
            PNR = reservation.PNR,
            TransactionId = $"TXN-{reservation.Id.ToString().Substring(0, 8)}",
            TransactionAmount = reservation.TotalPrice,
            Currency = reservation.Currency.ToString(),
            PaymentMethod = reservation.PaymentMethod.ToString(),
            TransactionType = "Payment",
            CreatedAt = reservation.CreatedDate,
            PaymentStatus = reservation.PaymentStatus.ToString()
        };
        
        return View(paymentDisplay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.RefundReservationAsync(id, ct);
        
        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }
        else
        {
            TempData["SuccessMessage"] = message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

// Helper DTO for displaying payments
public class PaymentDisplayDto
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public string PNR { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal TransactionAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
