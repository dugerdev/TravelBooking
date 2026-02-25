namespace TravelBooking.Web.DTOs.Tours;

public class CreateTourDto
{
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
    public List<string> Included { get; set; } = new();
    public string Difficulty { get; set; } = "Easy";
    public int MaxGroupSize { get; set; }
}
