using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.DTOs.Reservations;

public class CreateTicketDto
{
    public Guid FlightId { get; set; }
    public Guid? ReservationId { get; set; }
    public Guid PassengerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public BaggageOption BaggageOption { get; set; } = BaggageOption.Light;
    public decimal TicketPrice { get; set; }
    public decimal BaggageFee { get; set; }
    public string? SeatNumber { get; set; }
}
