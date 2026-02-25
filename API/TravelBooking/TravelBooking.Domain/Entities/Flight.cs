using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Events;
using System;
using System.Collections.Generic;

namespace TravelBooking.Domain.Entities;

//---Ucus Aggregate Root---//
public class Flight : BaseEntity, IAggregateRoot
{
    public string FlightNumber { get; private set; } = string.Empty;               //--- Ucus numarasi
    public string AirlineName { get; private set; } = string.Empty;                //--- Ucus sirketi adi

    public FlightType FlightType { get; private set; } = FlightType.Direct;        //--- Ucus turu
    public FlightRegion FlightRegion { get; private set; } = FlightRegion.Domestic; //--- Ucus bolgesi

    //--- Fiyatlandirma ve koltuk bilgileri ---
    public Money BasePrice { get; private set; } = null!;                          //--- Temel Bilet Fiyati (Not: EF Core uyumu icin null! eklenebilir)
    public int AvailableSeats { get; private set; }                                //--- Musait Koltuklar
    public int TotalSeats { get; private set; }                                     //--- Toplam Koltuk Sayisi

    //--- Rotasyon Bilgileri ---
    public Guid DepartureAirportId { get; private set; }                            //--- Kalkis Havalimani Id si
    public Airport DepartureAirport { get; private set; } = null!;                  //--- Kalkis Havalimani
    public Guid ArrivalAirportId { get; private set; }                              //--- Varis Havalimani Id si  
    public Airport ArrivalAirport { get; private set; } = null!;                   //--- Varis Havalimani

    //--- Zamanlar Cizelgesi ---
    public DateTime ScheduledDeparture { get; private set; }                        //--- Planlanan Kalkis Zamani
    public DateTime ScheduledArrival { get; private set; }                         //--- Planlanan Varis Zamani

    //--- Iliskiler (Guvenli Koleksiyon) ---
    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets;

    //--- Constructor ---
    protected Flight() { }

    //--- Ucusu gecerli bir durumda baslatan Parametreli Yapici Metot ---
    public Flight(
        string flightNumber,
        string airlineName,
        Guid departureAirportId,
        Guid arrivalAirportId,
        DateTime departure,
        DateTime arrival,
        Money basePrice,
        int totalSeats,
        FlightType type,
        FlightRegion region
        )
    {
        //---Temel Domain Invariant (Kural)---//
        if (arrival <= departure)
            throw new ArgumentException("Varis, ayrilistan sonra olmalidir.", nameof(arrival));

        //---Atamalar---//
        FlightNumber = flightNumber.Trim().ToUpperInvariant();
        AirlineName = airlineName.Trim();
        DepartureAirportId = departureAirportId;
        ArrivalAirportId = arrivalAirportId;
        ScheduledDeparture = departure;
        ScheduledArrival = arrival;
        BasePrice = basePrice;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        FlightType = type;
        FlightRegion = region;
    }

    /// <summary>
    /// Reserves the specified number of seats on the flight.
    /// </summary>
    /// <param name="count">The number of seats to reserve.</param>
    /// <exception cref="ArgumentException">Thrown when count is not positive.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there are not enough available seats.</exception>
    public void ReserveSeats(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Sayim pozitif olmalidir.", nameof(count));
        if (AvailableSeats < count)
            throw new InvalidOperationException("Yeterli sayida bos koltuk yok.");

        AvailableSeats -= count;
        AddDomainEvent(new SeatsReservedEvent(this.Id, count));
    }

    /// <summary>
    /// Releases the specified number of seats back to availability (for cancelled tickets).
    /// </summary>
    /// <param name="count">The number of seats to release.</param>
    /// <exception cref="ArgumentException">Thrown when count is not positive.</exception>
    /// <exception cref="InvalidOperationException">Thrown when releasing seats would exceed total seat count.</exception>
    public void ReleaseSeats(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Sayim pozitif olmalidir.", nameof(count));
        if (AvailableSeats + count > TotalSeats)
            throw new InvalidOperationException("Serbest birakilan koltuk sayisi toplam koltuk sayisini asamaz.");

        AvailableSeats += count;
        AddDomainEvent(new SeatReleasedEvent(this.Id, count));
    }

    /// <summary>
    /// Updates the flight schedule with new departure and arrival times.
    /// </summary>
    /// <param name="newDeparture">The new scheduled departure date and time.</param>
    /// <param name="newArrival">The new scheduled arrival date and time.</param>
    /// <exception cref="ArgumentException">Thrown when arrival time is not after departure time.</exception>
    public void UpdateSchedule(DateTime newDeparture, DateTime newArrival)
    {
        if (newArrival <= newDeparture)
            throw new ArgumentException("Varis, ayrilistan sonra olmalidir.", nameof(newArrival));

        ScheduledDeparture = newDeparture;
        ScheduledArrival = newArrival;
        AddDomainEvent(new FlightScheduleUpdatedEvent(this.Id, newDeparture, newArrival));
    }

    /// <summary>
    /// Updates the available seat count (typically used for external API synchronization).
    /// </summary>
    /// <param name="availableSeats">The new available seat count.</param>
    /// <exception cref="ArgumentException">Thrown when available seats is negative or exceeds total seats.</exception>
    public void UpdateAvailableSeats(int availableSeats)
    {
        if (availableSeats < 0)
            throw new ArgumentException("Musait koltuk sayisi negatif olamaz.", nameof(availableSeats));
        if (availableSeats > TotalSeats)
            throw new ArgumentException("Musait koltuk sayisi toplam koltuk sayisini asamaz.", nameof(availableSeats));

        AvailableSeats = availableSeats;
    }

    /// <summary>
    /// Adds a ticket to this flight.
    /// </summary>
    /// <param name="ticket">The ticket to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when ticket is null.</exception>
    public void AddTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (!_tickets.Contains(ticket))
            _tickets.Add(ticket);
    }
}