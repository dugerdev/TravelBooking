using TravelBooking.Web.DTOs.Enums;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Airports;

namespace TravelBooking.Web.DTOs.Flights;

public class FlightDto
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public FlightType FlightType { get; set; }
    public FlightRegion FlightRegion { get; set; }
    public decimal BasePriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Price => BasePriceAmount; // Alias for compatibility
    public MoneyDto? BasePrice => new MoneyDto { Amount = BasePriceAmount, Currency = Currency }; // For compatibility
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public Guid DepartureAirportId { get; set; }
    public string? DepartureAirportCode { get; set; }
    public AirportDto? DepartureAirport { get; set; }
    public string? DepartureAirportName { get; set; }
    public string? DepartureAirportIATA { get; set; }
    /// <summary>IATA code for display (nested or flat fallback).</summary>
    public string? DepartureIATA => DepartureAirport?.IATA_Code ?? DepartureAirportIATA ?? DepartureAirportCode;
    public Guid ArrivalAirportId { get; set; }
    public string? ArrivalAirportCode { get; set; }
    public AirportDto? ArrivalAirport { get; set; }
    public string? ArrivalAirportName { get; set; }
    public string? ArrivalAirportIATA { get; set; }
    /// <summary>IATA code for display (nested or flat fallback).</summary>
    public string? ArrivalIATA => ArrivalAirport?.IATA_Code ?? ArrivalAirportIATA ?? ArrivalAirportCode;
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
