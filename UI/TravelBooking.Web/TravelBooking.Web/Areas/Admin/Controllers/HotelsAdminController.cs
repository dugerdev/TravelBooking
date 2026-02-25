using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelBooking.Web.Services.Hotels;
using TravelBooking.Web.DTOs.Hotels;
using TravelBooking.Web.Helpers;
using Microsoft.Data.SqlClient;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HotelsController : Controller
{
    private readonly IHotelService _hotelService;
    private readonly IConfiguration _configuration;

    public HotelsController(IHotelService hotelService, IConfiguration configuration)
    {
        _hotelService = hotelService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minStarRating, decimal? maxPrice,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);
        
        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinStarRating = minStarRating;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.Message = null as string;

        if (!string.IsNullOrWhiteSpace(searchTerm) || minStarRating.HasValue || maxPrice.HasValue)
        {
            var (success, message, hotels) = await _hotelService.SearchAsync(searchTerm, minStarRating, maxPrice, ct);
            ViewBag.Message = message;
            var list = hotels ?? new List<HotelDto>();
            var totalCount = list.Count;
            var pagedItems = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var paged = new TravelBooking.Web.DTOs.Common.PagedResultDto<HotelDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(paged);
        }

        var (ok, msg, pagedResult) = await _hotelService.GetAllPagedAsync(pageNumber, pageSize, ct);
        ViewBag.Message = msg;
        if (!ok || pagedResult == null)
            return View(new TravelBooking.Web.DTOs.Common.PagedResultDto<HotelDto>());
        return View(pagedResult);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, hotel) = await _hotelService.GetByIdAsync(id, ct);
        
        if (!success || hotel == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        
        return View(hotel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateHotelDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHotelDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.City) || string.IsNullOrWhiteSpace(dto.Country))
        {
            ModelState.AddModelError("", "Name, City and Country are required.");
            return View(dto);
        }
        var (success, message) = await _hotelService.CreateAsync(dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Hotel created successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to create hotel.";
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, hotel) = await _hotelService.GetByIdAsync(id, ct);
        if (!success || hotel == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        // Standart oda fiyatını bul
        var standardRoom = hotel.Rooms?.FirstOrDefault(r => r.Type == "Standart Oda");
        
        var dto = new CreateHotelDto
        {
            Name = hotel.Name,
            City = hotel.City,
            Country = hotel.Country,
            Address = hotel.Address ?? "",
            StarRating = hotel.StarRating,
            PricePerNight = hotel.PricePerNight,
            Currency = hotel.Currency ?? "USD",
            ImageUrl = hotel.ImageUrl ?? "",
            Description = hotel.Description ?? "",
            HasFreeWifi = hotel.HasFreeWifi,
            HasParking = hotel.HasParking,
            HasPool = hotel.HasPool,
            HasRestaurant = hotel.HasRestaurant,
            StandardRoomPrice = standardRoom?.Price ?? 30m
        };
        ViewBag.HotelId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateHotelDto dto, CancellationToken ct = default)
    {
        dto.ImageUrl = ImageUrlHelper.NormalizeForSave(dto.ImageUrl);
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.City) || string.IsNullOrWhiteSpace(dto.Country))
        {
            ModelState.AddModelError("", "Name, City and Country are required.");
            return View(dto);
        }
        
        // Standart oda fiyatını güncelle (SQL ile direkt)
        if (dto.StandardRoomPrice.HasValue && dto.StandardRoomPrice.Value > 0)
        {
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connStr))
                {
                    TempData["ErrorMessage"] = "Veritabanı bağlantı bilgisi bulunamadı.";
                    return View(dto);
                }
                
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync(ct);
                
                // Önce standart oda var mı kontrol et
                var checkSql = "SELECT COUNT(*) FROM Rooms WHERE HotelId = @HotelId AND Type = 'Standart Oda'";
                using var checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@HotelId", id);
                var roomCount = (int)await checkCmd.ExecuteScalarAsync(ct);
                
                if (roomCount == 0)
                {
                    TempData["ErrorMessage"] = "Bu otelde standart oda bulunamadı. Önce oda eklemelisiniz.";
                    return View(dto);
                }
                
                // Fiyatı güncelle
                var updateSql = "UPDATE Rooms SET Price = @Price, Currency = @Currency WHERE HotelId = @HotelId AND Type = 'Standart Oda'";
                using var updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@Price", dto.StandardRoomPrice.Value);
                updateCmd.Parameters.AddWithValue("@Currency", dto.Currency);
                updateCmd.Parameters.AddWithValue("@HotelId", id);
                var rowsAffected = await updateCmd.ExecuteNonQueryAsync(ct);
                
                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = $"{rowsAffected} standart oda fiyatı güncellendi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Oda fiyatı güncellenemedi: {ex.Message}";
                return View(dto);
            }
        }
        
        var (success, message) = await _hotelService.UpdateAsync(id, dto, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Hotel updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to update hotel.";
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message, hotel) = await _hotelService.GetByIdAsync(id, ct);
        if (!success || hotel == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        return View(hotel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _hotelService.DeleteAsync(id, ct);
        if (success)
        {
            TempData["SuccessMessage"] = message ?? "Hotel deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["ErrorMessage"] = message ?? "Failed to delete hotel.";
        return RedirectToAction(nameof(Delete), new { id });
    }
}
