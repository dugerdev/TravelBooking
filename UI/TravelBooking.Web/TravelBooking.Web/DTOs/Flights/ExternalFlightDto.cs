namespace TravelBooking.Web.DTOs.Flights;

public class ExternalFlightDto
{
    public string ExternalFlightId { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string DepartureAirportIATA { get; set; } = string.Empty;
    public string? DepartureAirportName { get; set; }
    public string? DepartureCity { get; set; }
    public string? DepartureCountry { get; set; }
    public string ArrivalAirportIATA { get; set; } = string.Empty;
    public string? ArrivalAirportName { get; set; }
    public string? ArrivalCity { get; set; }
    public string? ArrivalCountry { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public decimal BasePriceAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string FlightType { get; set; } = "Direct";
    public string FlightRegion { get; set; } = "Domestic";
}
