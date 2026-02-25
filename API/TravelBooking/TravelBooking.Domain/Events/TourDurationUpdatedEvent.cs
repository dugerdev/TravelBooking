using System;

namespace TravelBooking.Domain.Events;

public class TourDurationUpdatedEvent : IDomainEvent
{
    public Guid TourId { get; }
    public int OldDuration { get; }
    public int NewDuration { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public TourDurationUpdatedEvent(Guid tourId, int oldDuration, int newDuration)
    {
        TourId = tourId;
        OldDuration = oldDuration;
        NewDuration = newDuration;
    }
}
