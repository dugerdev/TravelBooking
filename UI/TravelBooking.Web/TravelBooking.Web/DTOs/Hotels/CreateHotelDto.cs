namespace TravelBooking.Web.DTOs.Hotels;

/// <summary>DTO for creating or updating a hotel (API-compatible shape).</summary>
public class CreateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasFreeWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasPool { get; set; }
    public bool HasRestaurant { get; set; }
    
    // UI-only: Standart oda fiyati (listing sayfasinda gosterilir)
    public decimal? StandardRoomPrice { get; set; }
}
