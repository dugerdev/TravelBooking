using System;

namespace TravelBooking.Domain.Events;

public class RoomPriceUpdatedEvent : IDomainEvent
{
    public Guid RoomId { get; }
    public Guid HotelId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public RoomPriceUpdatedEvent(Guid roomId, Guid hotelId, decimal oldPrice, decimal newPrice)
    {
        RoomId = roomId;
        HotelId = hotelId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
