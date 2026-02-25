using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Flights;

public class PassengerFormViewModel
{
    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birth date is required")]
    public string BirthDate { get; set; } = string.Empty;

    public string? NationalNumber { get; set; }
    public string? PassportNumber { get; set; }

    // Extras
    public decimal MealPrice { get; set; } = 0;
    public decimal ExtraBaggagePrice { get; set; } = 0;
}
