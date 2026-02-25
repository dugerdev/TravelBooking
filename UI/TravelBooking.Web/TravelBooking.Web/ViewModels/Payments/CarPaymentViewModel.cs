using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Payments;

public class CarPaymentViewModel : IPaymentFormViewModel
{
    // Car Information (read-only)
    public Guid CarId { get; set; }
    public string CarModel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PickUpLocation { get; set; } = string.Empty;
    public string DropOffLocation { get; set; } = string.Empty;
    public DateTime PickUpDate { get; set; }
    public DateTime DropOffDate { get; set; }
    public int Days { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Transmission { get; set; } = string.Empty;
    public int Seats { get; set; }
    
    // Renter Information (from booking)
    public string RenterName { get; set; } = string.Empty;
    public string RenterEmail { get; set; } = string.Empty;
    public string RenterPhone { get; set; } = string.Empty;

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

    // Extras
    public bool HasKasko { get; set; }
    public decimal KaskoPrice { get; set; }
    public bool HasAdditionalDriver { get; set; }
    public decimal DriverPrice { get; set; }
    public int ChildSeatCount { get; set; }
    public decimal ChildSeatPrice { get; set; }
    public int BoosterSeatCount { get; set; }
    public decimal BoosterSeatPrice { get; set; }

    // Calculated totals
    public decimal Subtotal => PricePerDay * Days;
    public decimal ExtrasTotal => KaskoPrice + DriverPrice + (ChildSeatPrice * ChildSeatCount * Days) + (BoosterSeatPrice * BoosterSeatCount * Days);
    public decimal SubtotalWithExtras => Subtotal + ExtrasTotal;
    public decimal Tax => SubtotalWithExtras * 0.1m;
    public decimal Total => SubtotalWithExtras + Tax;
}
