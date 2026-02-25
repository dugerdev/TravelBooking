using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public class CreateCarDto
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public int Seats { get; set; }
    public int Doors { get; set; }
    public decimal PricePerDay { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string ImageUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool HasAirConditioning { get; set; }
    public bool HasGPS { get; set; }
}
