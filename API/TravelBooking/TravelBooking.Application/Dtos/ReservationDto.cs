using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class ReservationDto
{
    public Guid Id { get; set; }
    public string PNR { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? ReservationSummary { get; set; }
    public ReservationType ReservationType { get; set; }

    public Guid? HotelId { get; set; }
    public Guid? CarId { get; set; }
    public Guid? TourId { get; set; }

    public decimal TotalPrice { get; set; }
    public Currency Currency { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ReservationStatus Status { get; set; }

    public DateTime ReservationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedDate { get; set; }

    public List<TicketDto> Tickets { get; set; } = [];
    /// <summary>Passengers derived from tickets (for display).</summary>
    public List<PassengerDto> Passengers { get; set; } = [];

    /// <summary>Flight for flight reservations (first ticket's flight).</summary>
    public FlightDto? Flight { get; set; }

    /// <summary>Arac kiralama: alis tarihi.</summary>
    public DateTime? CarPickUpDate { get; set; }
    /// <summary>Arac kiralama: birakis tarihi.</summary>
    public DateTime? CarDropOffDate { get; set; }
    /// <summary>Arac kiralama: alis yeri.</summary>
    public string? CarPickUpLocation { get; set; }
    /// <summary>Arac kiralama: birakis yeri.</summary>
    public string? CarDropOffLocation { get; set; }

    /// <summary>Payments (including refunds) for this reservation.</summary>
    public List<PaymentDto> Payments { get; set; } = [];
}
