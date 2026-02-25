using System;
using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Events;

public class CarPriceUpdatedEvent : IDomainEvent
{
    public Guid CarId { get; }
    public Money OldPrice { get; }
    public Money NewPrice { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public CarPriceUpdatedEvent(Guid carId, Money oldPrice, Money newPrice)
    {
        CarId = carId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
