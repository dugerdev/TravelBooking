using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Flights;

public class CompleteBookingViewModel
{
    public string? ExternalFlightId { get; set; }
    public Guid? FlightId { get; set; }
    
    [Required]
    [Range(1, 9, ErrorMessage = "Passenger count must be between 1 and 9")]
    public int PassengerCount { get; set; } = 1;

    public List<PassengerFormViewModel> Passengers { get; set; } = [];

    // Payment Info
    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = "Card";

    [Required(ErrorMessage = "Card number is required")]
    [CreditCard(ErrorMessage = "Invalid card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Card holder name is required")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expiry date is required")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Invalid expiry date format (MM/YY)")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV is required")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Invalid CVV")]
    public string CVV { get; set; } = string.Empty;

    // Flight Details (for display)
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
