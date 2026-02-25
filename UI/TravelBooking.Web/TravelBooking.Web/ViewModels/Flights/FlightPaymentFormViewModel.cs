using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Flights;

/// <summary>Payment form only (booking data comes from Session).</summary>
public class FlightPaymentFormViewModel
{
    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = "Card";

    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CVV { get; set; }
}
