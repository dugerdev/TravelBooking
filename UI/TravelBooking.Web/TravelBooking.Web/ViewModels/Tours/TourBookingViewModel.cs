using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Tours;

public class TourBookingViewModel
{
    [Required]
    public int TourId { get; set; }

    public Guid TourRawId { get; set; }
    
    [Required]
    public string TourName { get; set; } = string.Empty;
    
    [Required]
    public string Destination { get; set; } = string.Empty;
    
    [Required]
    public int Duration { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime TourDate { get; set; }
    
    [Required]
    [Range(1, 20, ErrorMessage = "Participant count must be between 1 and 20")]
    public int ParticipantCount { get; set; } = 1;
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public string Currency { get; set; } = "USD";

    // Payment Info (optional on booking step; required on payment page)
    public string PaymentMethod { get; set; } = "Card";

    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CVV { get; set; }

    /// <summary>Passenger details: first element = primary contact, then 2nd, 3rd, ... participant.</summary>
    public List<TourPassengerViewModel> Passengers { get; set; } = new();
}
