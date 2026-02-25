using System;

namespace TravelBooking.Domain.Events;

//---Rezervasyon tamamlandiginda tetiklenen domain event---//
public class ReservationCompletedEvent : IDomainEvent
{
    public Guid ReservationId { get; }                                         //---Tamamlanan rezervasyonun kimligi---//
    public string PNR { get; }                                                 //---Tamamlanan rezervasyonun PNR kodu---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;                   //---Olayin gerceklestigi tarih ve saat---//

    public ReservationCompletedEvent(Guid reservationId, string pnr)
    {
        ReservationId = reservationId;
        PNR = pnr;
    }
}

