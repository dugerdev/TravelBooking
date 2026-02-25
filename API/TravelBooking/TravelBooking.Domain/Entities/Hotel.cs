using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Events;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a hotel aggregate root in the domain.
/// Manages hotel information, pricing, amenities, and room relationships.
/// </summary>
public class Hotel : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets the name of the hotel.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the city where the hotel is located.
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the country where the hotel is located.
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the address of the hotel.
    /// </summary>
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the star rating of the hotel (1-5).
    /// </summary>
    public int StarRating { get; private set; }

    /// <summary>
    /// Gets the price per night for the hotel.
    /// </summary>
    public Money PricePerNight { get; private set; } = null!;

    /// <summary>
    /// Gets the URL of the hotel's image.
    /// </summary>
    public string ImageUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of the hotel.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the average rating of the hotel (0-5).
    /// </summary>
    public double Rating { get; private set; }

    /// <summary>
    /// Gets the number of reviews for the hotel.
    /// </summary>
    public int ReviewCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has free WiFi.
    /// </summary>
    public bool HasFreeWifi { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has parking.
    /// </summary>
    public bool HasParking { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has a pool.
    /// </summary>
    public bool HasPool { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has a restaurant.
    /// </summary>
    public bool HasRestaurant { get; private set; }

    /// <summary>
    /// Gets the property type (Hotel, Apartment, Villa, Resort, etc.)
    /// </summary>
    public string PropertyType { get; private set; } = "Hotel";

    /// <summary>
    /// Gets a value indicating whether the hotel has air conditioning.
    /// </summary>
    public bool HasAirConditioning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has a fitness center.
    /// </summary>
    public bool HasFitnessCenter { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has a spa.
    /// </summary>
    public bool HasSpa { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel offers breakfast.
    /// </summary>
    public bool HasBreakfast { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has free cancellation.
    /// </summary>
    public bool HasFreeCancellation { get; private set; }

    /// <summary>
    /// Gets a value indicating whether no prepayment is required.
    /// </summary>
    public bool NoPrepaymentNeeded { get; private set; }

    /// <summary>
    /// Gets the distance from city center in kilometers.
    /// </summary>
    public double DistanceFromCenter { get; private set; }

    /// <summary>
    /// Gets the sustainability level (0-3, where 0 = not sustainable, 3+ = highly sustainable).
    /// </summary>
    public int SustainabilityLevel { get; private set; }

    /// <summary>
    /// Gets the brand name (if part of a hotel chain).
    /// </summary>
    public string? Brand { get; private set; }

    /// <summary>
    /// Gets the neighborhood/area name.
    /// </summary>
    public string? Neighbourhood { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the hotel has accessibility features.
    /// </summary>
    public bool HasAccessibilityFeatures { get; private set; }
    
    private readonly List<Room> _rooms = [];
    /// <summary>
    /// Gets the collection of rooms available in this hotel.
    /// </summary>
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Hotel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Hotel"/> class.
    /// </summary>
    /// <param name="name">The name of the hotel.</param>
    /// <param name="city">The city where the hotel is located.</param>
    /// <param name="country">The country where the hotel is located.</param>
    /// <param name="address">The address of the hotel.</param>
    /// <param name="starRating">The star rating (1-5).</param>
    /// <param name="pricePerNight">The price per night.</param>
    /// <param name="imageUrl">The URL of the hotel's image.</param>
    /// <param name="description">The description of the hotel.</param>
    /// <param name="hasFreeWifi">Whether the hotel has free WiFi.</param>
    /// <param name="hasParking">Whether the hotel has parking.</param>
    /// <param name="hasPool">Whether the hotel has a pool.</param>
    /// <param name="hasRestaurant">Whether the hotel has a restaurant.</param>
    /// <exception cref="ArgumentException">Thrown when star rating is not between 1 and 5.</exception>
    public Hotel(
        string name,
        string city,
        string country,
        string address,
        int starRating,
        Money pricePerNight,
        string imageUrl,
        string description,
        bool hasFreeWifi,
        bool hasParking,
        bool hasPool,
        bool hasRestaurant)
    {
        if (starRating < 1 || starRating > 5)
            throw new ArgumentException("Yildiz sayisi 1-5 arasinda olmalidir.", nameof(starRating));

        Name = name.Trim();
        City = city.Trim();
        Country = country.Trim();
        Address = address.Trim();
        StarRating = starRating;
        PricePerNight = pricePerNight;
        ImageUrl = imageUrl.Trim();
        Description = description.Trim();
        HasFreeWifi = hasFreeWifi;
        HasParking = hasParking;
        HasPool = hasPool;
        HasRestaurant = hasRestaurant;
        Rating = 0;
        ReviewCount = 0;
        
        // Initialize new properties with default values
        PropertyType = "Hotel";
        DistanceFromCenter = 0;
        SustainabilityLevel = 0;
        HasAirConditioning = false;
        HasFitnessCenter = false;
        HasSpa = false;
        HasBreakfast = false;
        HasFreeCancellation = false;
        NoPrepaymentNeeded = false;
        HasAccessibilityFeatures = false;
    }

    /// <summary>
    /// Updates the hotel information.
    /// </summary>
    /// <param name="name">The name of the hotel.</param>
    /// <param name="city">The city where the hotel is located.</param>
    /// <param name="country">The country where the hotel is located.</param>
    /// <param name="address">The address of the hotel.</param>
    /// <param name="starRating">The star rating (1-5).</param>
    /// <param name="pricePerNight">The price per night.</param>
    /// <param name="imageUrl">The URL of the hotel's image.</param>
    /// <param name="description">The description of the hotel.</param>
    /// <param name="hasFreeWifi">Whether the hotel has free WiFi.</param>
    /// <param name="hasParking">Whether the hotel has parking.</param>
    /// <param name="hasPool">Whether the hotel has a pool.</param>
    /// <param name="hasRestaurant">Whether the hotel has a restaurant.</param>
    /// <exception cref="ArgumentException">Thrown when star rating is not between 1 and 5.</exception>
    public void Update(
        string name,
        string city,
        string country,
        string address,
        int starRating,
        Money pricePerNight,
        string imageUrl,
        string description,
        bool hasFreeWifi,
        bool hasParking,
        bool hasPool,
        bool hasRestaurant)
    {
        if (starRating < 1 || starRating > 5)
            throw new ArgumentException("Yildiz sayisi 1-5 arasinda olmalidir.", nameof(starRating));

        var oldPrice = PricePerNight;
        var priceChanged = !oldPrice.Equals(pricePerNight);

        Name = name.Trim();
        City = city.Trim();
        Country = country.Trim();
        Address = address.Trim();
        StarRating = starRating;
        PricePerNight = pricePerNight;
        ImageUrl = imageUrl.Trim();
        Description = description.Trim();
        HasFreeWifi = hasFreeWifi;
        HasParking = hasParking;
        HasPool = hasPool;
        HasRestaurant = hasRestaurant;

        if (priceChanged)
            AddDomainEvent(new HotelPriceUpdatedEvent(this.Id, oldPrice, pricePerNight));
    }

    /// <summary>
    /// Updates the price per night for the hotel.
    /// Triggers a domain event when the price changes.
    /// </summary>
    /// <param name="newPrice">The new price per night.</param>
    /// <exception cref="ArgumentNullException">Thrown when newPrice is null.</exception>
    /// <exception cref="ArgumentException">Thrown when price amount is not positive.</exception>
    public void UpdatePricePerNight(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));
        if (newPrice.Amount <= 0)
            throw new ArgumentException("Fiyat 0'dan buyuk olmalidir.", nameof(newPrice));

        var oldPrice = PricePerNight;
        PricePerNight = newPrice;

        AddDomainEvent(new HotelPriceUpdatedEvent(this.Id, oldPrice, newPrice));
    }

    /// <summary>
    /// Adds a room to this hotel.
    /// Triggers a domain event when a room is added.
    /// </summary>
    /// <param name="room">The room to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    public void AddRoom(Room room)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        if (!_rooms.Contains(room))
        {
            _rooms.Add(room);
            AddDomainEvent(new RoomAddedEvent(this.Id, room.Id, room.Type));
        }
    }

    /// <summary>
    /// Removes a room from this hotel.
    /// </summary>
    /// <param name="room">The room to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when room is null.</exception>
    public void RemoveRoom(Room room)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        _rooms.Remove(room);
    }

    /// <summary>
    /// Updates the rating and review count for the hotel.
    /// </summary>
    /// <param name="newRating">The new average rating (0-5).</param>
    /// <param name="reviewCount">The new review count.</param>
    /// <exception cref="ArgumentException">Thrown when rating is not between 0 and 5, or review count is negative.</exception>
    public void UpdateRating(double newRating, int reviewCount)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating 0-5 arasinda olmalidir.", nameof(newRating));
        if (reviewCount < 0)
            throw new ArgumentException("Yorum sayisi negatif olamaz.", nameof(reviewCount));

        Rating = newRating;
        ReviewCount = reviewCount;
    }
}
