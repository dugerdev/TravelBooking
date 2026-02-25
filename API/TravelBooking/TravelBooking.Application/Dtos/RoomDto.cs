namespace TravelBooking.Application.Dtos;

public class RoomDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int MaxGuests { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = [];
    public bool IsAvailable { get; set; }
}
