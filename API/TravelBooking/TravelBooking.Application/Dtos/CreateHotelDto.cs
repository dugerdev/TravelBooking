using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public class CreateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasFreeWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasPool { get; set; }
    public bool HasRestaurant { get; set; }
}
