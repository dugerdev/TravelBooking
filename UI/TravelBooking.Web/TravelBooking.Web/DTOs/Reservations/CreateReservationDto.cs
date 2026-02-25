using TravelBooking.Web.DTOs.Enums;
using TravelBooking.Web.DTOs.Passengers;

namespace TravelBooking.Web.DTOs.Reservations;

public class CreateReservationDto
{
    public string AppUserId { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public ReservationType Type { get; set; } = ReservationType.Flight;
    
    public Guid? HotelId { get; set; }
    public Guid? CarId { get; set; }
    public Guid? TourId { get; set; }
    
    public string? PNR { get; set; }
    public List<CreateTicketDto> Tickets { get; set; } = new();
    /// <summary>Katilimcilar (Tur/Otel/Arac icin).</summary>
    public List<CreatePassengerDto> Participants { get; set; } = new();
    /// <summary>Harici ucus guzergahi (orn. Istanbul - Antalya).</summary>
    public string? FlightRouteSummary { get; set; }

    /// <summary>Arac kiralama: alis tarihi.</summary>
    public DateTime? CarPickUpDate { get; set; }
    /// <summary>Arac kiralama: birakis tarihi.</summary>
    public DateTime? CarDropOffDate { get; set; }
    /// <summary>Arac kiralama: alis yeri.</summary>
    public string? CarPickUpLocation { get; set; }
    /// <summary>Arac kiralama: birakis yeri.</summary>
    public string? CarDropOffLocation { get; set; }

    public CreatePaymentDto? Payment { get; set; }
}
