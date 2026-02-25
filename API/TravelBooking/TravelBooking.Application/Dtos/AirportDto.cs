namespace TravelBooking.Application.Dtos;

public sealed class AirportDto
{
    public Guid Id { get; set; }
    public string IATA_Code { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
