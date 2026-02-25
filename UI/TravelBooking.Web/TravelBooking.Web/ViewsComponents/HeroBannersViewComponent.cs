using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.ViewsComponents;

public class HeroBannersViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        await Task.CompletedTask;

        // Mock banner data - in future this could come from CMS/API
        List<BannerViewModel> banners =
        [
            new() { Title = "Discover Your Next Adventure", Subtitle = "Book flights to amazing destinations worldwide", ImageUrl = "/assets/img/hero-1.jpg", ButtonText = "Explore Now", ButtonLink = "/Flight/Listing" },
            new() { Title = "Summer Special Offers", Subtitle = "Save up to 40% on selected routes", ImageUrl = "/assets/img/hero-2.jpg", ButtonText = "View Deals", ButtonLink = "/Flight/Listing" },
            new() { Title = "Fly with Confidence", Subtitle = "Enhanced safety measures for your peace of mind", ImageUrl = "/assets/img/hero-3.jpg", ButtonText = "Learn More", ButtonLink = "/Pages/AboutUs" }
        ];

        return View(banners);
    }
}

public class BannerViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonLink { get; set; } = string.Empty;
}
