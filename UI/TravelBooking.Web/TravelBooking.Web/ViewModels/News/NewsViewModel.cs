namespace TravelBooking.Web.ViewModels.News;

public class NewsViewModel
{
    public int Id { get; set; }
    public Guid RawId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Travel Tips, Destinations, Company News, Industry
    public DateTime PublishDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/assets/img/news-default.jpg";
    public int ViewCount { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class NewsListingViewModel
{
    public List<NewsViewModel> News { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? Category { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}

public class NewsDetailViewModel
{
    public NewsViewModel News { get; set; } = new();
    public List<NewsViewModel> RelatedNews { get; set; } = [];
}
