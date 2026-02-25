using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public class CreateTourDto
{
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Price { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = [];
    public List<string> Included { get; set; } = [];
    public string Difficulty { get; set; } = "Easy";
    public int MaxGroupSize { get; set; }
}
