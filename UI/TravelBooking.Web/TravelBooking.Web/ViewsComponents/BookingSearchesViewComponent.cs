using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Flights;
using TravelBooking.Web.DTOs.Airports;

namespace TravelBooking.Web.ViewsComponents;

public class BookingSearchesViewComponent : ViewComponent
{
    private readonly IFlightService _flightService;

    public BookingSearchesViewComponent(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get popular airports for autocomplete suggestions
        var (success, message, airports) = await _flightService.SearchAirportsAsync("", 50, HttpContext.RequestAborted);
        
        var model = new BookingSearchViewModel
        {
            PopularAirports = success ? airports.Take(20).ToList() : new List<AirportDto>()
        };

        return View(model);
    }
}

public class BookingSearchViewModel
{
    public List<AirportDto> PopularAirports { get; set; } = new();
}
