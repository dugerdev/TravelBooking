using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Hotels;

namespace TravelBooking.Web.ViewComponents; 

public class PopularDestinationsViewComponent : ViewComponent
{
    private readonly IHotelService _hotelService;

    public PopularDestinationsViewComponent(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var (success, message, hotelsList) = await _hotelService.GetAllAsync(HttpContext.RequestAborted);
        
        if (!success || hotelsList == null || hotelsList.Count == 0)
        {
            // Veri yoksa bos liste dondur
            return View(new List<DTOs.Hotels.HotelDto>());
        }

        // En populer 8 oteli al (rating'e gore sirala)
        var popularHotels = hotelsList
            .OrderByDescending(h => h.Rating)
            .ThenByDescending(h => h.ReviewCount)
            .Take(8)
            .ToList();

        return View(popularHotels);
    }
}