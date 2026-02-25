using System;

namespace TravelBooking.Domain.Events;

public class RoomAddedEvent : IDomainEvent
{
    public Guid HotelId { get; }
    public Guid RoomId { get; }
    public string RoomType { get; }
    public DateTime DateOccurred { get; } = DateTime.UtcNow;

    public RoomAddedEvent(Guid hotelId, Guid roomId, string roomType)
    {
        HotelId = hotelId;
        RoomId = roomId;
        RoomType = roomType;
    }
}
