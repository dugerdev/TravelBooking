using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class CreateFlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;

    public Guid DepartureAirportId { get; set; }
    public Guid ArrivalAirportId { get; set; }

    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }

    public decimal BasePriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;

    public int TotalSeats { get; set; }

    public FlightType FlightType { get; set; } = FlightType.Direct;
    public FlightRegion FlightRegion { get; set; } = FlightRegion.Domestic;
}

