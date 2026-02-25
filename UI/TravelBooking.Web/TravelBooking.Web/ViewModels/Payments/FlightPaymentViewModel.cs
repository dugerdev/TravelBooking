using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Payments;

public class FlightPaymentViewModel : IPaymentFormViewModel
{
    // Flight Information (read-only)
    public Guid FlightId { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public int AdultCount { get; set; }
    public int ChildCount { get; set; }
    public int InfantCount { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    
    // Passenger Information (from booking)
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;

    // Payment Information
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

    // Billing Address
    [Required(ErrorMessage = "Billing address is required")]
    public string BillingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal code is required")]
    public string PostalCode { get; set; } = string.Empty;

    // Terms & Conditions
    [Range(typeof(bool), "true", "true", ErrorMessage = "Sartlari ve kosullari kabul etmelisiniz")]
    public bool AcceptTerms { get; set; }

    // Calculated totals
    public decimal Subtotal => Price;
    public decimal Tax => Subtotal * 0.1m;
    public decimal Total => Subtotal + Tax;
    public int TotalPassengers => AdultCount + ChildCount + InfantCount;
}
