using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Admin;

public class EditFlightViewModel
{
    public Guid Id { get; set; }
    
    [Display(Name = "Ucus Numarasi")]
    public string FlightNumber { get; set; } = string.Empty;
    
    [Display(Name = "Havayolu")]
    public string AirlineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kalkis tarihi gereklidir")]
    [Display(Name = "Kalkis Tarihi")]
    public DateTime ScheduledDeparture { get; set; }

    [Required(ErrorMessage = "Varis tarihi gereklidir")]
    [Display(Name = "Varis Tarihi")]
    public DateTime ScheduledArrival { get; set; }
}
