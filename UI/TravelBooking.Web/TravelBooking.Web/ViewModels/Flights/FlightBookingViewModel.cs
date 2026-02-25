namespace TravelBooking.Web.ViewModels.Flights;

public class FlightBookingViewModel
{
    public string ExternalFlightId { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public string? DepartureAirportIATA { get; set; }
    public string? ArrivalAirportIATA { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public int BaggageWeight { get; set; } = 20;
    public int StopCount { get; set; }
    public string CabinClass { get; set; } = "Economy";
    public string TotalDuration => ScheduledArrival > ScheduledDeparture
        ? $"{(int)(ScheduledArrival - ScheduledDeparture).TotalHours}s {(int)(ScheduledArrival - ScheduledDeparture).Minutes}dk"
        : "-";

    /// <summary>Gidis-donus ise true.</summary>
    public bool IsRoundTrip { get; set; }
    /// <summary>Donus ucusu fiyati (kisi basi).</summary>
    public decimal ReturnPrice { get; set; }
    public string? ReturnFlightNumber { get; set; }
    public string? ReturnAirlineName { get; set; }
    public string? ReturnDepartureCity { get; set; }
    public string? ReturnArrivalCity { get; set; }
    public DateTime? ReturnScheduledDeparture { get; set; }
    public DateTime? ReturnScheduledArrival { get; set; }
    /// <summary>Rezervasyon icin kullanilacak toplam bilet fiyati (gidis + donus, kisi basi).</summary>
    public decimal TotalPrice => IsRoundTrip ? Price + ReturnPrice : Price;
}
