using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels;
using TravelBooking.Web.Services.Flights;

namespace TravelBooking.Web.Controllers;

public class SearchController : Controller
{
    private readonly IFlightService _flightService;

    public SearchController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public async Task<IActionResult> Results(string q, CancellationToken ct = default)
    {
        var model = new SearchResultViewModel
        {
            SearchQuery = q ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(q))
        {
            // Search flights
            var (flightSuccess, flightMessage, flights) = await _flightService.SearchHybridAsync(
                q, q, null, null, null, ct);
            
            if (flightSuccess)
            {
                model.Flights = flights.Take(5).Select(f => new Models.Flight
                {
                    Id = (int)(f.Id.GetHashCode() & 0x7FFFFFFF),
                    FlightNumber = f.FlightNumber,
                    AirlineName = f.AirlineName,
                    DepartureCity = f.DepartureAirportCode ?? "Unknown",
                    ArrivalCity = f.ArrivalAirportCode ?? "Unknown",
                    DepartureTime = f.ScheduledDeparture,
                    ArrivalTime = f.ScheduledArrival,
                    Price = f.Price
                }).ToList();
            }

            // Search airports (as destinations)
            var (airportSuccess, airportMessage, airports) = await _flightService.SearchAirportsAsync(q, 5, ct);
            
            // Mock data for other categories (News, Cars, Tours)
            // In future, these would come from their respective services
            model.NewsItems = new List<Models.News>();
            model.Cars = new List<Models.Car>();
            model.ToursItems = new List<Models.Tours>();
        }

        return View(model);
    }
}