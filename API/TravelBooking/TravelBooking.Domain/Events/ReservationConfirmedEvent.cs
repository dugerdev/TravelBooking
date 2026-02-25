using System;

namespace TravelBooking.Domain.Events;

//---Rezervasyon onaylandiginda tetiklenen domain event---//
public class ReservationConfirmedEvent : IDomainEvent
{
    public Guid ReservationId { get; }                               //---Onaylanan rezervasyonun kimligi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;         //---Olayin gerceklestigi tarih ve saat---//

    public ReservationConfirmedEvent(Guid reservationId)
    {
        ReservationId = reservationId;
    }   
}
