using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Pages;

public class ContactViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }
}

public class AboutUsViewModel
{
    public int TotalFlights { get; set; }
    public int TotalCustomers { get; set; }
    public int YearsOfExperience { get; set; }
    public int CountriesServed { get; set; }
    public int ApprovedTestimonialsCount { get; set; }
    public string? SampleTestimonialComment { get; set; }
    public string? SampleTestimonialName { get; set; }
}
