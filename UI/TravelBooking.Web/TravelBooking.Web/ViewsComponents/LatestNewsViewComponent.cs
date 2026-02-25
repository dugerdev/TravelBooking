using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.News;
using TravelBooking.Web.DTOs.News;

namespace TravelBooking.Web.ViewComponents;

public class LatestNewsViewComponent : ViewComponent
{
    private readonly INewsService _newsService;

    public LatestNewsViewComponent(INewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var (success, message, newsList) = await _newsService.GetPublishedAsync(HttpContext.RequestAborted);
        
        if (!success || newsList == null || newsList.Count == 0)
        {
            // Veri yoksa bos liste dondur
            return View(new List<NewsDto>());
        }

        // En son 3 haberi al
        var latestNews = newsList.OrderByDescending(n => n.PublishDate).Take(3).ToList();
        
        return View(latestNews);
    }
}