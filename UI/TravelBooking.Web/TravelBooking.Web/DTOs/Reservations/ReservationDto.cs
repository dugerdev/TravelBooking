using TravelBooking.Web.DTOs.Enums;
using TravelBooking.Web.DTOs.Flights;
using TravelBooking.Web.DTOs.Passengers;

namespace TravelBooking.Web.DTOs.Reservations;

public class ReservationDto
{
    public Guid Id { get; set; }
    public string PNR { get; set; } = string.Empty;
    public string AppUserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? ReservationSummary { get; set; }
    public ReservationType ReservationType { get; set; } = ReservationType.Flight;
    
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
    public List<TicketDto> Tickets { get; set; } = new();

    /// <summary>Alias for TotalPrice for view compatibility.</summary>
    public decimal TotalAmount { get => TotalPrice; set => TotalPrice = value; }

    /// <summary>Alias for CreatedDate for view compatibility.</summary>
    public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }

    /// <summary>Flight info when reservation includes a flight (e.g. from first ticket).</summary>
    public FlightDto? Flight { get; set; }

    /// <summary>Arac kiralama: alis tarihi.</summary>
    public DateTime? CarPickUpDate { get; set; }
    /// <summary>Arac kiralama: birakis tarihi.</summary>
    public DateTime? CarDropOffDate { get; set; }
    /// <summary>Arac kiralama: alis yeri.</summary>
    public string? CarPickUpLocation { get; set; }
    /// <summary>Arac kiralama: birakis yeri.</summary>
    public string? CarDropOffLocation { get; set; }

    /// <summary>Payment summary for display.</summary>
    public ReservationPaymentInfo? Payment { get; set; }

    /// <summary>Passengers (from tickets or API).</summary>
    public List<PassengerDto> Passengers { get; set; } = new();

    /// <summary>Payments (including refunds) for this reservation.</summary>
    public List<PaymentDto> Payments { get; set; } = new();
}
