using System;

namespace TravelBooking.Domain.Events;

//---Koltuklar rezerve edildiginde tetiklenen domain event---//
public class SeatsReservedEvent : IDomainEvent
{
    public Guid FlightId { get; }                                    //---Rezerve edilen ucusun kimligi---//
    public int ReservedCount { get; }                                 //---Rezerve edilen koltuk sayisi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;         //---Olayin gerceklestigi tarih ve saat---//

    public SeatsReservedEvent(Guid flightId, int reservedCount)
    {
        FlightId = flightId;
        ReservedCount = reservedCount;
    }
}