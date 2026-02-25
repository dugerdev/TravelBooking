using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Flights;

/// <summary>Booking step 1: passenger details only (no payment). Posted when user clicks "Next" to go to Payment page.</summary>
public class FlightBookingStep1ViewModel
{
    public string? ExternalFlightId { get; set; }
    public Guid? FlightId { get; set; }

    [Required]
    [Range(1, 9, ErrorMessage = "Passenger count must be between 1 and 9")]
    public int PassengerCount { get; set; } = 1;

    [Required]
    [MinLength(1, ErrorMessage = "At least one passenger is required")]
    public List<PassengerFormViewModel> Passengers { get; set; } = [];

    public string FlightNumber { get; set; } = string.Empty;
    public string AirlineName { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string CabinClass { get; set; } = "Economy";
}
