using System;

namespace TravelBooking.Domain.Events;

//---Arac kiralama rezervasyonu yapildiginda tetiklenen domain event---//
public class CarRentedEvent : IDomainEvent
{
    public Guid ReservationId { get; }                                    //---Rezervasyon kimligi---//
    public Guid CarId { get; }                                            //---Kiralanan arac kimligi---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;             //---Olayin gerceklestigi tarih ve saat---//

    public CarRentedEvent(Guid reservationId, Guid carId)
    {
        ReservationId = reservationId;
        CarId = carId;
    }
}
