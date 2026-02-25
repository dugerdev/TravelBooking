using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class CreateReservationDto
{
    public string AppUserId { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public ReservationType Type { get; set; } = ReservationType.Flight;

    // Optional. If empty, API will generate a PNR.
    public string? PNR { get; set; }

    //---Hotel/Car/Tour iliskileri---//
    public Guid? HotelId { get; set; }
    public Guid? CarId { get; set; }
    public Guid? TourId { get; set; }

    //---Biletler---//
    public List<CreateTicketDto> Tickets { get; set; } = [];

    //---Katilimcilar (Tur/Otel/Arac icin)---//
    public List<CreatePassengerDto> Participants { get; set; } = [];

    /// <summary>Harici ucus guzergahi (orn. "Istanbul - Antalya").</summary>
    public string? FlightRouteSummary { get; set; }

    /// <summary>Arac kiralama: alis tarihi.</summary>
    public DateTime? CarPickUpDate { get; set; }
    /// <summary>Arac kiralama: birakis tarihi.</summary>
    public DateTime? CarDropOffDate { get; set; }
    /// <summary>Arac kiralama: alis yeri.</summary>
    public string? CarPickUpLocation { get; set; }
    /// <summary>Arac kiralama: birakis yeri.</summary>
    public string? CarDropOffLocation { get; set; }

    //---Odeme bilgileri---//
    public CreatePaymentDto? Payment { get; set; }
}

