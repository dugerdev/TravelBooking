using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.ViewModels.Admin;
using TravelBooking.Web.DTOs.Flights;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FlightsAdminController : Controller
{
    private readonly IAdminService _adminService;

    public FlightsAdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var (success, message, paged) = await _adminService.GetFlightsPagedAsync(pageNumber, pageSize, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            return View(new DTOs.Common.PagedResultDto<DTOs.Flights.FlightDto>());
        }

        return View(paged ?? new DTOs.Common.PagedResultDto<DTOs.Flights.FlightDto>());
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, flight) = await _adminService.GetFlightByIdAsync(id, ct);
        
        if (!success || flight == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(flight);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var (success, message, airports) = await _adminService.GetAllAirportsAsync(ct);
        
        ViewBag.Airports = airports ?? new List<TravelBooking.Web.DTOs.Airports.AirportDto>();
        
        return View(new CreateFlightDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFlightDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var (success2, message2, airports2) = await _adminService.GetAllAirportsAsync(ct);
            ViewBag.Airports = airports2 ?? new List<TravelBooking.Web.DTOs.Airports.AirportDto>();
            return View(dto);
        }

        var (success, message) = await _adminService.CreateFlightAsync(dto, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            var (success3, message3, airports3) = await _adminService.GetAllAirportsAsync(ct);
            ViewBag.Airports = airports3 ?? new List<TravelBooking.Web.DTOs.Airports.AirportDto>();
            return View(dto);
        }

        TempData["Success"] = "Uçuş başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, flight) = await _adminService.GetFlightByIdAsync(id, ct);
        
        if (!success || flight == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        var model = new EditFlightViewModel
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            AirlineName = flight.AirlineName,
            ScheduledDeparture = flight.ScheduledDeparture,
            ScheduledArrival = flight.ScheduledArrival
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFlightViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new UpdateFlightDto
        {
            ScheduledDeparture = model.ScheduledDeparture,
            ScheduledArrival = model.ScheduledArrival
        };

        var (success, message) = await _adminService.UpdateFlightAsync(model.Id, dto, ct);
        
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["Success"] = "Uçuş başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message, flight) = await _adminService.GetFlightByIdAsync(id, ct);
        
        if (!success || flight == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(flight);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.DeleteFlightAsync(id, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Uçuş başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }
}
