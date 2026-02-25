using System;
using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Events;

public class TourPriceUpdatedEvent : IDomainEvent
{
    public Guid TourId { get; }
    public Money OldPrice { get; }
    public Money NewPrice { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public TourPriceUpdatedEvent(Guid tourId, Money oldPrice, Money newPrice)
    {
        TourId = tourId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
