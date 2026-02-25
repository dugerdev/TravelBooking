using System;

namespace TravelBooking.Domain.Events;

//---Bilet iptal edildiginde tetiklenen domain event---//
public class TicketCancelledEvent : IDomainEvent
{
    public Guid TicketId { get; }                                              //---Iptal edilen biletin kimligi---//
    public Guid FlightId { get; }                                              //---Biletin ait oldugu ucusun kimligi---//
    public Guid ReservationId { get; }                                         //---Biletin ait oldugu rezervasyonun kimligi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;                   //---Olayin gerceklestigi tarih ve saat---//

    public TicketCancelledEvent(Guid ticketId, Guid flightId, Guid reservationId)
    {
        TicketId = ticketId;
        FlightId = flightId;
        ReservationId = reservationId;
    }
}

