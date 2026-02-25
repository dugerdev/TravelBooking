using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class TicketDto
{
    public Guid Id { get; set; }
    public Guid FlightId { get; set; }
    public Guid ReservationId { get; set; }
    public Guid PassengerId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;

    public SeatClass SeatClass { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public BaggageOption BaggageOption { get; set; }

    public decimal TicketPrice { get; set; }
    public decimal BaggageFee { get; set; }

    public TicketStatus TicketStatus { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedDate { get; set; }

    /// <summary>Ticket number for display (e.g. from domain).</summary>
    public string TicketNumber { get; set; } = string.Empty;
    /// <summary>Passenger details for display.</summary>
    public PassengerDto? Passenger { get; set; }
}
