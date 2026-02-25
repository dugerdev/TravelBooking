using TravelBooking.Domain.Common;
using TravelBooking.Domain.Events;
using System.Linq;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Room entity representing a hotel room
/// </summary>
public class Room : BaseEntity
{
    public Guid HotelId { get; private set; }
    public Hotel Hotel { get; private set; } = null!;
    public string Type { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int MaxGuests { get; private set; }
    public string Description { get; private set; } = string.Empty;
    
    private readonly List<string> _features = [];
    public IReadOnlyCollection<string> Features => _features.AsReadOnly();
    
    public bool IsAvailable { get; private set; } = true;

    protected Room() { }

    public Room(
        Guid hotelId,
        string type,
        decimal price,
        int maxGuests,
        string description,
        List<string> features,
        string currency = "USD")
    {
        if (maxGuests <= 0)
            throw new ArgumentException("Maksimum misafir sayisi pozitif olmalidir.", nameof(maxGuests));
        if (price < 0)
            throw new ArgumentException("Fiyat negatif olamaz.", nameof(price));

        HotelId = hotelId;
        Type = type.Trim();
        Price = price;
        Currency = currency;
        MaxGuests = maxGuests;
        Description = description.Trim();
        IsAvailable = true;

        if (features != null)
            _features.AddRange(features.Select(f => f.Trim()));
    }

    /// <summary>
    /// Updates the room information.
    /// </summary>
    /// <param name="type">The type of the room.</param>
    /// <param name="price">The price of the room.</param>
    /// <param name="maxGuests">The maximum number of guests.</param>
    /// <param name="description">The description of the room.</param>
    /// <param name="features">The list of features available in the room.</param>
    /// <exception cref="ArgumentException">Thrown when maxGuests is not positive or price is negative.</exception>
    public void Update(
        string type,
        decimal price,
        int maxGuests,
        string description,
        List<string> features,
        string? currency = null)
    {
        if (maxGuests <= 0)
            throw new ArgumentException("Maksimum misafir sayisi pozitif olmalidir.", nameof(maxGuests));
        if (price < 0)
            throw new ArgumentException("Fiyat negatif olamaz.", nameof(price));

        var oldPrice = Price;
        var priceChanged = oldPrice != price;

        Type = type.Trim();
        Price = price;
        if (!string.IsNullOrEmpty(currency))
            Currency = currency;
        MaxGuests = maxGuests;
        Description = description.Trim();

        _features.Clear();
        if (features != null)
            _features.AddRange(features.Select(f => f.Trim()));

        if (priceChanged)
            AddDomainEvent(new RoomPriceUpdatedEvent(this.Id, HotelId, oldPrice, price));
    }

    /// <summary>
    /// Updates the price of the room.
    /// Triggers a domain event when the price changes.
    /// </summary>
    /// <param name="newPrice">The new price.</param>
    /// <exception cref="ArgumentException">Thrown when price is negative.</exception>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Fiyat negatif olamaz.", nameof(newPrice));

        var oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new RoomPriceUpdatedEvent(this.Id, HotelId, oldPrice, newPrice));
    }

    /// <summary>
    /// Marks the room as available.
    /// </summary>
    public void MarkAsAvailable()
    {
        if (IsAvailable)
            return;

        IsAvailable = true;
    }

    /// <summary>
    /// Marks the room as unavailable.
    /// </summary>
    public void MarkAsUnavailable()
    {
        if (!IsAvailable)
            return;

        IsAvailable = false;
    }

    /// <summary>
    /// Adds a feature to the room.
    /// </summary>
    /// <param name="feature">The feature to add.</param>
    /// <exception cref="ArgumentException">Thrown when feature is null or whitespace.</exception>
    public void AddFeature(string feature)
    {
        if (string.IsNullOrWhiteSpace(feature))
            throw new ArgumentException("Ozellik bos olamaz.", nameof(feature));

        var trimmed = feature.Trim();
        if (!_features.Contains(trimmed))
            _features.Add(trimmed);
    }

    /// <summary>
    /// Removes a feature from the room.
    /// </summary>
    /// <param name="feature">The feature to remove.</param>
    public void RemoveFeature(string feature)
    {
        if (string.IsNullOrWhiteSpace(feature))
            return;

        _features.Remove(feature.Trim());
    }

    /// <summary>
    /// Sets the navigation property to the hotel.
    /// </summary>
    /// <param name="hotel">The hotel to associate with this room.</param>
    internal void SetHotel(Hotel hotel)
    {
        Hotel = hotel;
    }
}
