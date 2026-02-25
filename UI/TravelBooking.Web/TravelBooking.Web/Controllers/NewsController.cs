using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.News;
using TravelBooking.Web.Services.News;

namespace TravelBooking.Web.Controllers;

public class NewsController : Controller
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<IActionResult> Listing(string? query, string? category, int page = 1, CancellationToken ct = default)
    {
        const int pageSize = 6;
        var (success, message, newsList) = await _newsService.SearchAsync(query, category, ct);

        var newsViewModels = newsList.Select(n => new NewsViewModel
        {
            Id = (int)(n.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = n.Id,
            Title = n.Title,
            Summary = n.Summary,
            Content = n.Content,
            Category = n.Category,
            PublishDate = n.PublishDate,
            Author = n.Author,
            ImageUrl = n.ImageUrl,
            ViewCount = n.ViewCount,
            Tags = n.Tags
        }).ToList();

        var totalPages = (int)Math.Ceiling(newsViewModels.Count / (double)pageSize);
        var pagedNews = newsViewModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var model = new NewsListingViewModel
        {
            News = pagedNews,
            SearchQuery = query,
            Category = category,
            CurrentPage = page,
            TotalPages = totalPages
        };

        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }

        return View(model);
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken ct = default)
    {
        var (success, message, newsDto) = await _newsService.GetByIdAsync(id, ct);
        
        if (!success || newsDto == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Listing));
        }

        var news = new NewsViewModel
        {
            Id = (int)(newsDto.Id.GetHashCode() & 0x7FFFFFFF),
            RawId = newsDto.Id,
            Title = newsDto.Title,
            Summary = newsDto.Summary,
            Content = newsDto.Content,
            Category = newsDto.Category,
            PublishDate = newsDto.PublishDate,
            Author = newsDto.Author,
            ImageUrl = newsDto.ImageUrl,
            ViewCount = newsDto.ViewCount,
            Tags = newsDto.Tags
        };

        // Get related news from same category
        var (relSuccess, _, allNews) = await _newsService.GetPublishedAsync(ct);
        var relatedNews = relSuccess 
            ? allNews.Where(n => n.Id != id && n.Category == newsDto.Category)
                    .Take(3)
                    .Select(n => new NewsViewModel
                    {
                        Id = (int)(n.Id.GetHashCode() & 0x7FFFFFFF),
                        RawId = n.Id,
                        Title = n.Title,
                        Summary = n.Summary,
                        ImageUrl = n.ImageUrl,
                        PublishDate = n.PublishDate
                    }).ToList()
            : new List<NewsViewModel>();

        var model = new NewsDetailViewModel
        {
            News = news,
            RelatedNews = relatedNews
        };

        return View(model);
    }
}
