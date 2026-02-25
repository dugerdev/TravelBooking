using System;

namespace TravelBooking.Domain.Events;

//---Ucus programi guncellendiginde tetiklenen domain event---//
public class FlightScheduleUpdatedEvent : IDomainEvent
{
    public Guid FlightId { get; }                                    //---Guncellenen ucusun kimligi---//
    public DateTime NewDeparture { get; }                            //---Yeni kalkis zamani---//
    public DateTime NewArrival { get; }                              //---Yeni varis zamani---//
    public DateTime DateOccurred { get; } = DateTime.UtcNow;         //---Olayin gerceklestigi tarih ve saat---//

    public FlightScheduleUpdatedEvent(Guid flightId, DateTime newDeparture, DateTime newArrival)
    {
        FlightId = flightId;
        NewDeparture = newDeparture;
        NewArrival = newArrival;
    }
}
