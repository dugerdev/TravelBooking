using TravelBooking.Web.DTOs.Reservations;

namespace TravelBooking.Web.ViewModels.Flights;

public class CheckInViewModel
{
    public ReservationDto? Reservation { get; set; }
    public string PNR { get; set; } = string.Empty;
    public List<string> AssignedSeats { get; set; } = new();
    public DateTime CheckInTime { get; set; }
}
