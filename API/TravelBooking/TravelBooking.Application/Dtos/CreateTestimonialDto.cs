namespace TravelBooking.Application.Dtos;

public class CreateTestimonialDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? AvatarUrl { get; set; }
}
