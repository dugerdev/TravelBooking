using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelBooking.Web.Services.Cars;
using TravelBooking.Web.DTOs.Cars;
using TravelBooking.Web.Helpers;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CarsController : Controller
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    public async Task<IActionResult> Index(string? searchLocation, string? category, decimal? maxPrice,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);
        
        ViewBag.SearchLocation = searchLocation;
        ViewBag.Category = category;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.Message = null as string;

        if (!string.IsNullOrWhiteSpace(searchLocation) || !string.IsNullOrWhiteSpace(category) || maxPrice.HasValue)
        {
            var (success, message, cars) = await _carService.SearchAsync(searchLocation, category, maxPrice, ct);
            ViewBag.Message = message;
            var list = cars ?? new List<CarDto>();
            var totalCount = list.Count;
            var pagedItems = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var paged = new TravelBooking.Web.DTOs.Common.PagedResultDto<CarDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(paged);
        }

        var (ok, msg, pagedResult) = await _carService.GetAllPagedAsync(pageNumber, pageSize, ct);
        ViewBag.Message = msg;
        if (!ok || pagedResult == null)
            return View(new TravelBooking.Web.DTOs.Common.PagedResultDto<CarDto>());
        return View(pagedResult);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, car) = await _carService.GetByIdAsync(id, ct);
        
        if (!success || car == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        
        return View(car);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateCarDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCarDto dto, CancellationToken ct = default)
    {
        dto.ImageUrl = ImageUrlHelper.NormalizeForSave(dto.ImageUrl);
        if (string.IsNullOrWhiteSpace(dto.Brand) || string.IsNullOrWhiteSpace(dto.Model))
        {
            ModelState.AddModelError("", "Brand and Model are required.");
            return View(dto);
        }
        var (success, message) = await _carService.CreateAsync(dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Car created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to create car.";
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, car) = await _carService.GetByIdAsync(id, ct);
        if (!success || car == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        var dto = new CreateCarDto
        {
            Brand = car.Brand,
            Model = car.Model,
            Category = car.Category ?? "",
            Year = car.Year,
            FuelType = car.FuelType ?? "",
            Transmission = car.Transmission ?? "",
            Seats = car.Seats,
            Doors = car.Doors,
            PricePerDay = car.PricePerDay,
            Currency = car.Currency ?? "USD",
            ImageUrl = car.ImageUrl ?? "",
            Location = car.Location ?? "",
            HasAirConditioning = car.HasAirConditioning,
            HasGPS = car.HasGPS
        };
        ViewBag.CarId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateCarDto dto, CancellationToken ct = default)
    {
        dto.ImageUrl = ImageUrlHelper.NormalizeForSave(dto.ImageUrl);
        if (string.IsNullOrWhiteSpace(dto.Brand) || string.IsNullOrWhiteSpace(dto.Model))
        {
            ModelState.AddModelError("", "Brand and Model are required.");
            return View(dto);
        }
        var (success, message) = await _carService.UpdateAsync(id, dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Car updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to update car.";
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message, car) = await _carService.GetByIdAsync(id, ct);
        if (!success || car == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        return View(car);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _carService.DeleteAsync(id, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Car deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to delete car.";
        return RedirectToAction(nameof(Delete), new { id });
    }
}
