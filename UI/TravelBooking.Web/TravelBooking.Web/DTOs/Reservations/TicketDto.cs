using TravelBooking.Web.DTOs.Enums;
using TravelBooking.Web.DTOs.Passengers;

namespace TravelBooking.Web.DTOs.Reservations;

public class TicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid FlightId { get; set; }
    public Guid ReservationId { get; set; }
    public Guid PassengerId { get; set; }
    public PassengerDto? Passenger { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public SeatClass SeatClass { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public BaggageOption BaggageOption { get; set; }
    public decimal TicketPrice { get; set; }
    public decimal BaggageFee { get; set; }

    /// <summary>Alias for TicketPrice for view compatibility.</summary>
    public decimal Price { get => TicketPrice; set => TicketPrice = value; }
    public TicketStatus TicketStatus { get; set; }
    public TicketStatus Status => TicketStatus; // Alias for compatibility
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedDate { get; set; }
}
