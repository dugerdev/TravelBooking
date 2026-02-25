using System;

namespace TravelBooking.Domain.Events;

public class CarAvailabilityChangedEvent : IDomainEvent
{
    public Guid CarId { get; }
    public bool IsAvailable { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public CarAvailabilityChangedEvent(Guid carId, bool isAvailable)
    {
        CarId = carId;
        IsAvailable = isAvailable;
    }
}
