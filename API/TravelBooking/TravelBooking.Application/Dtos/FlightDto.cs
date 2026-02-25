using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class FlightDto
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public FlightType FlightType { get; set; }
    public FlightRegion FlightRegion { get; set; }

    public decimal BasePriceAmount { get; set; }
    public string Currency { get; set; } = "USD";

    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }

    public Guid DepartureAirportId { get; set; }
    public Guid ArrivalAirportId { get; set; }
    public AirportDto? DepartureAirport { get; set; }
    public AirportDto? ArrivalAirport { get; set; }
    /// <summary>Kalkis havalimani adi (nested yoksa fallback).</summary>
    public string? DepartureAirportName { get; set; }
    /// <summary>Varis havalimani adi (nested yoksa fallback).</summary>
    public string? ArrivalAirportName { get; set; }
    /// <summary>Kalkis havalimani IATA kodu (nested yoksa fallback).</summary>
    public string? DepartureAirportIATA { get; set; }
    /// <summary>Varis havalimani IATA kodu (nested yoksa fallback).</summary>
    public string? ArrivalAirportIATA { get; set; }

    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }

    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
