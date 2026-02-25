using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelBooking.Web.Services.Tours;
using TravelBooking.Web.DTOs.Tours;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ToursController : Controller
{
    private readonly ITourService _tourService;

    public ToursController(ITourService tourService)
    {
        _tourService = tourService;
    }

    public async Task<IActionResult> Index(string? searchDestination, int? minDuration, int? maxDuration,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);
        
        ViewBag.SearchDestination = searchDestination;
        ViewBag.MinDuration = minDuration;
        ViewBag.MaxDuration = maxDuration;
        ViewBag.Message = null as string;

        if (!string.IsNullOrWhiteSpace(searchDestination) || minDuration.HasValue || maxDuration.HasValue)
        {
            var (success, message, tours) = await _tourService.SearchAsync(searchDestination, minDuration, maxDuration, ct);
            ViewBag.Message = message;
            var list = tours ?? new List<TourDto>();
            var totalCount = list.Count;
            var pagedItems = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var paged = new TravelBooking.Web.DTOs.Common.PagedResultDto<TourDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(paged);
        }

        var (ok, msg, pagedResult) = await _tourService.GetAllPagedAsync(pageNumber, pageSize, ct);
        ViewBag.Message = msg;
        if (!ok || pagedResult == null)
            return View(new TravelBooking.Web.DTOs.Common.PagedResultDto<TourDto>());
        return View(pagedResult);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, tour) = await _tourService.GetByIdAsync(id, ct);
        if (!success || tour == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        return View(tour);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTourDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTourDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Destination))
        {
            ModelState.AddModelError("", "Name and Destination are required.");
            return View(dto);
        }
        if (dto.Duration <= 0) dto.Duration = 1;
        if (dto.MaxGroupSize <= 0) dto.MaxGroupSize = 10;
        var (success, message) = await _tourService.CreateAsync(dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Tour created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to create tour.";
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, tour) = await _tourService.GetByIdAsync(id, ct);
        if (!success || tour == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        var dto = new CreateTourDto
        {
            Name = tour.Name,
            Destination = tour.Destination,
            Duration = tour.Duration,
            Price = tour.Price,
            Currency = tour.Currency ?? "USD",
            ImageUrl = tour.ImageUrl ?? "",
            Description = tour.Description ?? "",
            Highlights = tour.Highlights ?? new List<string>(),
            Included = tour.Included ?? new List<string>(),
            Difficulty = tour.Difficulty ?? "Easy",
            MaxGroupSize = tour.MaxGroupSize
        };
        ViewBag.TourId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateTourDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Destination))
        {
            ModelState.AddModelError("", "Name and Destination are required.");
            return View(dto);
        }
        if (dto.Duration <= 0) dto.Duration = 1;
        if (dto.MaxGroupSize <= 0) dto.MaxGroupSize = 10;
        var (success, message) = await _tourService.UpdateAsync(id, dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Tour updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to update tour.";
        ViewBag.TourId = id;
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message, tour) = await _tourService.GetByIdAsync(id, ct);
        if (!success || tour == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        return View(tour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _tourService.DeleteAsync(id, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Tour deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to delete tour.";
        return RedirectToAction(nameof(Delete), new { id });
    }
}
