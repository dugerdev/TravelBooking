using TravelBooking.Web.DTOs.Reservations;

namespace TravelBooking.Web.ViewModels.Flights;

public class ManageBookingViewModel
{
    public ReservationDto? Reservation { get; set; }
    public string PNR { get; set; } = string.Empty;
}
