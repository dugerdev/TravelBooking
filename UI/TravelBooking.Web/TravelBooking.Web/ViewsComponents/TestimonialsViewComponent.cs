using TravelBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.ViewsComponents;

public class TestimonialsViewComponent(TestimonialService testimonialService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var result = await testimonialService.GetApprovedTestimonialsAsync();
        
        if (result.Success && result.Data != null && result.Data.Any())
        {
            var testimonials = result.Data
                .OrderByDescending(t => t.CreatedDate)
                .Take(6)
                .Select(t => new TestimonialViewModel
                {
                    Name = t.CustomerName,
                    Rating = t.Rating,
                    Comment = t.Comment,
                    Location = t.Location,
                    AvatarUrl = t.AvatarUrl
                })
                .ToList();
            
            return View(testimonials);
        }

        // Fallback to mock data if no testimonials in database (avatars under wwwroot/assets/img)
        List<TestimonialViewModel> mockTestimonials =
        [
            new() { Name = "John Doe", Rating = 5, Comment = "Excellent service! Best flight booking experience.", Location = "New York, USA", AvatarUrl = "/assets/img/avatar-1.jpg" },
            new() { Name = "Jane Smith", Rating = 5, Comment = "Very easy to use and great prices.", Location = "London, UK", AvatarUrl = "/assets/img/avatar-2.jpg" },
            new() { Name = "Ali Yilmaz", Rating = 4, Comment = "Harika bir deneyimdi, tesekkurler!", Location = "Istanbul, Turkiye", AvatarUrl = "/assets/img/avatar-3.jpg" },
            new() { Name = "Maria Garcia", Rating = 5, Comment = "Highly recommended! Will use again.", Location = "Madrid, Spain", AvatarUrl = "/assets/img/avatar-4.jpg" }
        ];

        return View(mockTestimonials);
    }
}

public class TestimonialViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}