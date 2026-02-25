using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Payments;

public class HotelPaymentViewModel : IPaymentFormViewModel
{
    // Hotel Information (read-only)
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights { get; set; }
    public int Rooms { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = "TRY";
    
    // Guest Information (from booking)
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;

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
    public decimal Subtotal => PricePerNight * Nights * Rooms;
    public decimal Tax => Subtotal * 0.1m;
    public decimal Total => Subtotal + Tax;
}
