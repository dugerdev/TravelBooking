using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.ViewsComponents;

public class BenefitsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        await Task.CompletedTask;

        // Static benefits data
        List<BenefitViewModel> benefits =
        [
            new() { Icon = "fa-shield-alt", Title = "Secure Booking", Description = "Your payment information is protected with industry-leading security" },
            new() { Icon = "fa-tags", Title = "Best Prices", Description = "We guarantee the best prices on all flights" },
            new() { Icon = "fa-clock", Title = "24/7 Support", Description = "Our customer service team is always here to help" },
            new() { Icon = "fa-plane-departure", Title = "Wide Selection", Description = "Choose from thousands of flights worldwide" }
        ];

        return View(benefits);
    }
}

public class BenefitViewModel
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

