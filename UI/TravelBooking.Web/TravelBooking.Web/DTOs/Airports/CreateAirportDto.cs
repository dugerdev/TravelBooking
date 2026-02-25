namespace TravelBooking.Web.DTOs.Airports;

public class CreateAirportDto
{
    public string IATA_Code { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
