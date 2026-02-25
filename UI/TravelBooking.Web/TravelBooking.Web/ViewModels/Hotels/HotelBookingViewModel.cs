using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Hotels;

public class HotelBookingViewModel
{
    [Required]
    public int HotelId { get; set; }

    /// <summary>Actual hotel GUID for detail link (not posted).</summary>
    public Guid RawHotelId { get; set; }
    
    [Required]
    public int RoomId { get; set; }
    
    [Required]
    public string HotelName { get; set; } = string.Empty;
    
    [Required]
    public string RoomType { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckIn { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOut { get; set; }
    
    [Required]
    [Range(1, 10, ErrorMessage = "Guest count must be between 1 and 10")]
    public int Guests { get; set; } = 1;
    
    [Required]
    public decimal TotalPrice { get; set; }
    
    [Required]
    public string Currency { get; set; } = "TRY";

    // Payment Info (optional on booking step; required on payment page)
    public string PaymentMethod { get; set; } = "Card";

    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CVV { get; set; }

    // Contact Info
    [Required(ErrorMessage = "Contact name is required")]
    public string ContactName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Optional ID or Passport number for check-in.</summary>
    [Display(Name = "ID / Passport Number")]
    public string? IdOrPassport { get; set; }
}
