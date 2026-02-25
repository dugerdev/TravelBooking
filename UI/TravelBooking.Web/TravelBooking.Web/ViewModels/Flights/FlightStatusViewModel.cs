using TravelBooking.Web.DTOs.Flights;

namespace TravelBooking.Web.ViewModels.Flights;

public class FlightStatusViewModel
{
    public string? FlightNumber { get; set; }
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public string? Date { get; set; }
    public List<FlightDto> Flights { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
