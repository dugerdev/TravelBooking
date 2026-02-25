namespace TravelBooking.Application.Dtos;

public class TourDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = [];
    public List<string> Included { get; set; } = [];
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int MaxGroupSize { get; set; }
    public DateTime CreatedDate { get; set; }
}
