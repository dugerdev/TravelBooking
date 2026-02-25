namespace TravelBooking.Web.DTOs.Airports;

public class AirportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string IATA_Code { get; set; } = string.Empty;
    public string ICAO_Code { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
