using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Tours;
using TravelBooking.Web.DTOs.Tours;

namespace TravelBooking.Web.ViewComponents; 

public class LatestFlightsViewComponent : ViewComponent
{
    private readonly ITourService _tourService;

    public LatestFlightsViewComponent(ITourService tourService)
    {
        _tourService = tourService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get latest tours to display as featured deals
        var (success, message, tours) = await _tourService.GetAllAsync(HttpContext.RequestAborted);

        var model = success ? tours.Take(6).ToList() : new List<TourDto>();
        return View(model);
    }
}