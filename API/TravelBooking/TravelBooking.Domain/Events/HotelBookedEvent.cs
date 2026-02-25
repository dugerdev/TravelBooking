using System;

namespace TravelBooking.Domain.Events;

//---Otel rezervasyonu yapildiginda tetiklenen domain event---//
public class HotelBookedEvent : IDomainEvent
{
    public Guid ReservationId { get; }                                    //---Rezervasyon kimligi---//
    public Guid HotelId { get; }                                          //---Rezerve edilen otel kimligi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;             //---Olayin gerceklestigi tarih ve saat---//

    public HotelBookedEvent(Guid reservationId, Guid hotelId)
    {
        ReservationId = reservationId;
        HotelId = hotelId;
    }
}
