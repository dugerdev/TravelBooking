using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.DTOs.Airports;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AirportsAdminController : Controller
{
    private readonly IAdminService _adminService;

    public AirportsAdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var (success, message, airports) = await _adminService.GetAllAirportsAsync(ct);
        if (!success)
        {
            TempData["Error"] = message;
            return View(new List<AirportDto>());
        }
        return View(airports ?? new List<AirportDto>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string IATA_Code, string Name, string City, string Country, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(IATA_Code) || string.IsNullOrWhiteSpace(Name) || 
            string.IsNullOrWhiteSpace(City) || string.IsNullOrWhiteSpace(Country))
        {
            TempData["Error"] = "Tüm alanlar zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        var createDto = new CreateAirportDto
        {
            IATA_Code = IATA_Code.ToUpper().Trim(),
            Name = Name.Trim(),
            City = City.Trim(),
            Country = Country.Trim()
        };

        var (success, message) = await _adminService.CreateAirportAsync(createDto, ct);
        
        if (success)
        {
            TempData["Success"] = "Havalimanı başarıyla eklendi.";
        }
        else
        {
            TempData["Error"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, airport) = await _adminService.GetAirportByIdAsync(id, ct);
        if (!success || airport == null)
        {
            TempData["Error"] = message ?? "Havalimanı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(airport);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateAirportDto dto, CancellationToken ct = default)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.City) || string.IsNullOrWhiteSpace(dto.Country))
        {
            TempData["Error"] = "Tüm alanlar zorunludur.";
            var (_, __, airport) = await _adminService.GetAirportByIdAsync(id, ct);
            if (airport != null)
                return View(new AirportDto { Id = airport.Id, IATA_Code = airport.IATA_Code, Name = dto?.Name ?? airport.Name, City = dto?.City ?? airport.City, Country = dto?.Country ?? airport.Country });
            return RedirectToAction(nameof(Index));
        }

        var (success, message) = await _adminService.UpdateAirportAsync(id, dto, ct);
        if (success)
            TempData["Success"] = message;
        else
            TempData["Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message, airport) = await _adminService.GetAirportByIdAsync(id, ct);
        if (!success || airport == null)
        {
            TempData["Error"] = message ?? "Havalimanı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }
        return View(airport);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.DeleteAirportAsync(id, ct);
        if (success)
            TempData["Success"] = message;
        else
            TempData["Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
