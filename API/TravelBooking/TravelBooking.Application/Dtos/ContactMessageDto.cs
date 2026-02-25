namespace TravelBooking.Application.Dtos;

public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string? ReadBy { get; set; }
    public string? Response { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? ResponseBy { get; set; }
}
