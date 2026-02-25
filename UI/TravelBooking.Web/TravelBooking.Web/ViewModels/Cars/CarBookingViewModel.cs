using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Cars;

public class CarBookingViewModel
{
    public int CarId { get; set; }

    /// <summary>Actual car GUID for detail link (not posted).</summary>
    public Guid RawCarId { get; set; }
    
    public string Brand { get; set; } = string.Empty;
    
    public string Model { get; set; } = string.Empty;

    /// <summary>Car image URL for display on booking page.</summary>
    public string? ImageUrl { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime PickupDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime ReturnDate { get; set; }
    
    public string PickupLocation { get; set; } = string.Empty;
    
    public string ReturnLocation { get; set; } = string.Empty;
    
    public decimal TotalPrice { get; set; }
    
    public string Currency { get; set; } = "TRY";

    // Payment Info (optional for pay-at-pickup)
    public string PaymentMethod { get; set; } = "Card";

    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CVV { get; set; }

    // Contact Info (Passenger 1 Details style)
    [Required(ErrorMessage = "Cinsiyet gereklidir")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100)]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100)]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ulke gereklidir")]
    [StringLength(10)]
    [Display(Name = "Ulke")]
    public string Country { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Dogum Tarihi")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi girin")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon gereklidir")]
    [RegularExpression(@"^[0-9\s\-]{9,15}$", ErrorMessage = "Telefon 9-15 karakter olmalidir (orn: 555 123 4567)")]
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>Optional ID or Passport number for rental agreement.</summary>
    [Display(Name = "ID / Passport Number")]
    public string? IdOrPassport { get; set; }

    /// <summary>Driver's license serial number (Ehliyet Seri No) - required for car rental.</summary>
    [Required(ErrorMessage = "Ehliyet seri numarasi gereklidir")]
    [Display(Name = "Ehliyet Seri No")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Ehliyet seri numarasi 6-20 karakter olmalidir")]
    [RegularExpression(@"^[A-Za-z0-9\s\-]+$", ErrorMessage = "Sadece harf, rakam, tire ve bosluk kullanin")]
    public string DriverLicenseSerial { get; set; } = string.Empty;

    /// <summary>Kullanim sartlari ve gizlilik politikasi kabulu (odeme adimina gecmek icin zorunlu).</summary>
    [Display(Name = "Kullanim Sartlari")]
    public bool AcceptTerms { get; set; }

    // Extras (Optional Add-ons)
    public bool HasKasko { get; set; }
    public bool HasAdditionalDriver { get; set; }
    public int ChildSeatCount { get; set; }
    public int BoosterSeatCount { get; set; }
}
