using System;
using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Events;

public class HotelPriceUpdatedEvent : IDomainEvent
{
    public Guid HotelId { get; }
    public Money OldPrice { get; }
    public Money NewPrice { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public HotelPriceUpdatedEvent(Guid hotelId, Money oldPrice, Money newPrice)
    {
        HotelId = hotelId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
