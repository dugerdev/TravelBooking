using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Flights;

namespace TravelBooking.Web.ViewsComponents;

public class SpecialOffersViewComponent : ViewComponent
{
    private readonly IFlightService _flightService;

    public SpecialOffersViewComponent(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get flights with special offers (mock: just get some flights)
        var (success, message, flights) = await _flightService.SearchHybridAsync(
            null, null, null, null, null, HttpContext.RequestAborted);

        // Take random flights as "special offers"
        var offers = success ? flights.Take(4).ToList() : new List<DTOs.Flights.FlightDto>();
        
        return View(offers);
    }
}
