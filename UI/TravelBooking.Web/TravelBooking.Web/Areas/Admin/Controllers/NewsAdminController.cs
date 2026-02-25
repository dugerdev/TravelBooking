using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelBooking.Web.Services.News;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class NewsController : Controller
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);
        
        var (success, message, paged) = await _newsService.GetAllPagedAsync(pageNumber, pageSize, ct);
        ViewBag.Message = message;
        
        return View(paged ?? new TravelBooking.Web.DTOs.Common.PagedResultDto<TravelBooking.Web.DTOs.News.NewsDto>());
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct = default)
    {
        var (success, message, news) = await _newsService.GetByIdAsync(id, ct);
        
        if (!success || news == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
        
        return View(news);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TravelBooking.Web.DTOs.News.CreateNewsDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TravelBooking.Web.DTOs.News.CreateNewsDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var (success, message) = await _newsService.CreateAsync(dto, ct);
        
        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Haber başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct = default)
    {
        var (success, message, news) = await _newsService.GetByIdAsync(id, ct);
        
        if (!success || news == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        var dto = new TravelBooking.Web.DTOs.News.CreateNewsDto
        {
            Title = news.Title,
            Summary = news.Summary,
            Content = news.Content,
            Category = news.Category,
            PublishDate = news.PublishDate,
            Author = news.Author,
            ImageUrl = news.ImageUrl,
            Tags = news.Tags,
            IsPublished = news.IsPublished
        };
        
        ViewBag.NewsId = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TravelBooking.Web.DTOs.News.CreateNewsDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.NewsId = id;
            return View(dto);
        }

        var (success, message) = await _newsService.UpdateAsync(id, dto, ct);
        
        if (!success)
        {
            TempData["ErrorMessage"] = message;
            ViewBag.NewsId = id;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Haber başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var (success, message) = await _newsService.DeleteAsync(id, ct);
        
        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }
        else
        {
            TempData["SuccessMessage"] = "Haber başarıyla silindi.";
        }

        return RedirectToAction(nameof(Index));
    }
}
