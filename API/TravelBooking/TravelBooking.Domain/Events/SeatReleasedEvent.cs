using System;

namespace TravelBooking.Domain.Events;

//---Koltuklar serbest birakildiginda tetiklenen domain event---//
public class SeatReleasedEvent : IDomainEvent
{
    public Guid FlightId { get; }                                              //---Koltuklari serbest birakilan ucusun kimligi---//
    public int ReleasedCount { get; }                                          //---Serbest birakilan koltuk sayisi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;                   //---Olayin gerceklestigi tarih ve saat---//

    public SeatReleasedEvent(Guid flightId, int releasedCount)
    {
        FlightId = flightId;
        ReleasedCount = releasedCount;
    }
}

