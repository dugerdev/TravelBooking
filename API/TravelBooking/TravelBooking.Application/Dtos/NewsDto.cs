namespace TravelBooking.Application.Dtos;

public class NewsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public List<string> Tags { get; set; } = [];
    public bool IsPublished { get; set; }
    public DateTime CreatedDate { get; set; }
}
