using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class CreateTicketDto
{
    public Guid FlightId { get; set; }
    /// <summary>Yeni rezervasyon olustururken backend doldurur; frontend null veya atlayabilir.</summary>
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
