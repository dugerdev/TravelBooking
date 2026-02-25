using TravelBooking.Web.DTOs.Flights;

namespace TravelBooking.Web.ViewModels.Flights;

public class FlightListingViewModel
{
    public List<ExternalFlightDto> Flights { get; set; } = [];
    public List<ExternalFlightDto> ReturnFlights { get; set; } = [];
    public FlightSearchViewModel Search { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? InfoMessage { get; set; }

    public bool IsRoundTrip => Search?.Way == "round-trip";
}
