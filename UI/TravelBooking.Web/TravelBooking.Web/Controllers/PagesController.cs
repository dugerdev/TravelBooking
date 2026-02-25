using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.Pages;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.Services.ContactMessages;
using TravelBooking.Web.Models;
using TravelBooking.Web.Services;
using TravelBooking.Web.DTOs.Testimonials;
using System.Linq;

namespace TravelBooking.Web.Controllers;

public class PagesController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IContactMessageService _contactMessageService;
    private readonly TestimonialService _testimonialService;

    public PagesController(IAdminService adminService, IContactMessageService contactMessageService, TestimonialService testimonialService)
    {
        _adminService = adminService;
        _contactMessageService = contactMessageService;
        _testimonialService = testimonialService;
    }

    public async Task<IActionResult> AboutUs(CancellationToken ct = default)
    {
        var (success, message, stats) = await _adminService.GetDashboardStatisticsAsync(ct);
        var testimonialsResult = await _testimonialService.GetApprovedTestimonialsAsync();
        var testimonials = testimonialsResult.Success && testimonialsResult.Data != null ? testimonialsResult.Data : new List<TestimonialDto>();
        var count = testimonials.Count;
        var first = testimonials.OrderByDescending(t => t.CreatedDate).FirstOrDefault();

        var model = new AboutUsViewModel
        {
            TotalFlights = stats?.TotalFlights ?? 1250,
            TotalCustomers = stats?.TotalUsers ?? 15000,
            YearsOfExperience = 15,
            CountriesServed = 120,
            ApprovedTestimonialsCount = count,
            SampleTestimonialComment = first?.Comment,
            SampleTestimonialName = first?.CustomerName
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Create contact message
            var contactMessage = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone ?? string.Empty,
                Subject = model.Subject,
                Message = model.Message
            };

            var (success, message) = await _contactMessageService.CreateAsync(contactMessage, ct);
            
            if (success)
            {
                TempData["ContactSuccess"] = "Thank you for contacting us! We'll get back to you soon.";
                return RedirectToAction(nameof(Contact));
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View(model);
        }
    }

    public IActionResult PrivacyPolicy()
    {
        return View();
    }
}
