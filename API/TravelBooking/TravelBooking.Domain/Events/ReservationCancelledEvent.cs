using System;

namespace TravelBooking.Domain.Events;

//---Rezervasyon iptal edildiginde tetiklenen domain event---//
public class ReservationCancelledEvent : IDomainEvent
{
    public Guid ReservationId { get; }                                         //---Iptal edilen rezervasyonun kimligi---//
    public string PNR { get; }                                                 //---Iptal edilen rezervasyonun PNR kodu---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;                   //---Olayin gerceklestigi tarih ve saat---//

    public ReservationCancelledEvent(Guid reservationId, string pnr)
    {
        ReservationId = reservationId;
        PNR = pnr;
    }
}

