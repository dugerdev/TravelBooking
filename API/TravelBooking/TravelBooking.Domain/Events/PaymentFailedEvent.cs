using System;

namespace TravelBooking.Domain.Events;

//---Odeme basarisiz oldugunda tetiklenen domain event---//
public class PaymentFailedEvent : IDomainEvent
{
    public Guid ReservationId { get; }                                         //---Odemesi basarisiz olan rezervasyonun kimligi---//
    public string PNR { get; }                                                 //---Rezervasyon PNR kodu---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;                   //---Olayin gerceklestigi tarih ve saat---//

    public PaymentFailedEvent(Guid reservationId, string pnr)
    {
        ReservationId = reservationId;
        PNR = pnr;
    }
}

