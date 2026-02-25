using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Events;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a car aggregate root in the domain.
/// Manages car rental information, pricing, features, and availability.
/// </summary>
public class Car : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets the brand of the car.
    /// </summary>
    public string Brand { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the model of the car.
    /// </summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the category of the car (e.g., Economy, Luxury, SUV).
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the manufacturing year of the car.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the fuel type of the car (e.g., Petrol, Diesel, Electric).
    /// </summary>
    public string FuelType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transmission type (e.g., Manual, Automatic).
    /// </summary>
    public string Transmission { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the number of seats in the car.
    /// </summary>
    public int Seats { get; private set; }

    /// <summary>
    /// Gets the number of doors on the car.
    /// </summary>
    public int Doors { get; private set; }

    /// <summary>
    /// Gets the price per day for renting the car.
    /// </summary>
    public Money PricePerDay { get; private set; } = null!;

    /// <summary>
    /// Gets the URL of the car's image.
    /// </summary>
    public string ImageUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the location where the car is available for rental.
    /// </summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the car has air conditioning.
    /// </summary>
    public bool HasAirConditioning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the car has GPS navigation.
    /// </summary>
    public bool HasGPS { get; private set; }

    /// <summary>
    /// Gets the average rating of the car (0-5).
    /// </summary>
    public double Rating { get; private set; }

    /// <summary>
    /// Gets the number of reviews for the car.
    /// </summary>
    public int ReviewCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the car is currently available for rental.
    /// </summary>
    public bool IsAvailable { get; private set; } = true;

    /// <summary>
    /// Gets the mileage policy (Unlimited, Limited).
    /// </summary>
    public string MileagePolicy { get; private set; } = "Unlimited";

    /// <summary>
    /// Gets the fuel policy (Full to Full, Same to Same).
    /// </summary>
    public string FuelPolicy { get; private set; } = "Full to Full";

    /// <summary>
    /// Gets the pick-up location type (In Terminal, Shuttle, Meet & Greet).
    /// </summary>
    public string PickupLocationType { get; private set; } = "In Terminal";

    /// <summary>
    /// Gets the supplier/rental company name.
    /// </summary>
    public string Supplier { get; private set; } = string.Empty;

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Car() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Car"/> class.
    /// </summary>
    /// <param name="brand">The brand of the car.</param>
    /// <param name="model">The model of the car.</param>
    /// <param name="category">The category of the car.</param>
    /// <param name="year">The manufacturing year.</param>
    /// <param name="fuelType">The fuel type.</param>
    /// <param name="transmission">The transmission type.</param>
    /// <param name="seats">The number of seats.</param>
    /// <param name="doors">The number of doors.</param>
    /// <param name="pricePerDay">The price per day.</param>
    /// <param name="imageUrl">The URL of the car's image.</param>
    /// <param name="location">The rental location.</param>
    /// <param name="hasAirConditioning">Whether the car has air conditioning.</param>
    /// <param name="hasGPS">Whether the car has GPS.</param>
    /// <exception cref="ArgumentException">Thrown when year, seats, or doors are invalid.</exception>
    public Car(
        string brand,
        string model,
        string category,
        int year,
        string fuelType,
        string transmission,
        int seats,
        int doors,
        Money pricePerDay,
        string imageUrl,
        string location,
        bool hasAirConditioning,
        bool hasGPS)
    {
        if (year < 1900 || year > DateTime.Now.Year + 1)
            throw new ArgumentException("Gecersiz yil.", nameof(year));
        if (seats <= 0)
            throw new ArgumentException("Koltuk sayisi pozitif olmalidir.", nameof(seats));
        if (doors <= 0)
            throw new ArgumentException("Kapi sayisi pozitif olmalidir.", nameof(doors));

        Brand = brand.Trim();
        Model = model.Trim();
        Category = category.Trim();
        Year = year;
        FuelType = fuelType.Trim();
        Transmission = transmission.Trim();
        Seats = seats;
        Doors = doors;
        PricePerDay = pricePerDay;
        ImageUrl = imageUrl.Trim();
        Location = location.Trim();
        HasAirConditioning = hasAirConditioning;
        HasGPS = hasGPS;
        Rating = 0;
        ReviewCount = 0;
        IsAvailable = true;
        
        // Initialize new properties with default values
        MileagePolicy = "Unlimited";
        FuelPolicy = "Full to Full";
        PickupLocationType = "In Terminal";
        Supplier = string.Empty;
    }

    /// <summary>
    /// Updates the car information.
    /// </summary>
    /// <param name="brand">The brand of the car.</param>
    /// <param name="model">The model of the car.</param>
    /// <param name="category">The category of the car.</param>
    /// <param name="year">The manufacturing year.</param>
    /// <param name="fuelType">The fuel type.</param>
    /// <param name="transmission">The transmission type.</param>
    /// <param name="seats">The number of seats.</param>
    /// <param name="doors">The number of doors.</param>
    /// <param name="pricePerDay">The price per day.</param>
    /// <param name="imageUrl">The URL of the car's image.</param>
    /// <param name="location">The rental location.</param>
    /// <param name="hasAirConditioning">Whether the car has air conditioning.</param>
    /// <param name="hasGPS">Whether the car has GPS.</param>
    /// <exception cref="ArgumentException">Thrown when year, seats, or doors are invalid.</exception>
    public void Update(
        string brand,
        string model,
        string category,
        int year,
        string fuelType,
        string transmission,
        int seats,
        int doors,
        Money pricePerDay,
        string imageUrl,
        string location,
        bool hasAirConditioning,
        bool hasGPS)
    {
        if (year < 1900 || year > DateTime.Now.Year + 1)
            throw new ArgumentException("Gecersiz yil.", nameof(year));
        if (seats <= 0)
            throw new ArgumentException("Koltuk sayisi pozitif olmalidir.", nameof(seats));
        if (doors <= 0)
            throw new ArgumentException("Kapi sayisi pozitif olmalidir.", nameof(doors));

        var oldPrice = PricePerDay;
        var priceChanged = !oldPrice.Equals(pricePerDay);

        Brand = brand.Trim();
        Model = model.Trim();
        Category = category.Trim();
        Year = year;
        FuelType = fuelType.Trim();
        Transmission = transmission.Trim();
        Seats = seats;
        Doors = doors;
        PricePerDay = pricePerDay;
        ImageUrl = imageUrl.Trim();
        Location = location.Trim();
        HasAirConditioning = hasAirConditioning;
        HasGPS = hasGPS;

        if (priceChanged)
            AddDomainEvent(new CarPriceUpdatedEvent(this.Id, oldPrice, pricePerDay));
    }

    /// <summary>
    /// Updates the price per day for the car.
    /// Triggers a domain event when the price changes.
    /// </summary>
    /// <param name="newPrice">The new price per day.</param>
    /// <exception cref="ArgumentNullException">Thrown when newPrice is null.</exception>
    /// <exception cref="ArgumentException">Thrown when price amount is not positive.</exception>
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));
        if (newPrice.Amount <= 0)
            throw new ArgumentException("Fiyat 0'dan buyuk olmalidir.", nameof(newPrice));

        var oldPrice = PricePerDay;
        PricePerDay = newPrice;

        AddDomainEvent(new CarPriceUpdatedEvent(this.Id, oldPrice, newPrice));
    }

    /// <summary>
    /// Marks the car as available for rental.
    /// Triggers a domain event when availability changes.
    /// </summary>
    public void MarkAsAvailable()
    {
        if (IsAvailable)
            return;

        IsAvailable = true;
        AddDomainEvent(new CarAvailabilityChangedEvent(this.Id, true));
    }

    /// <summary>
    /// Marks the car as unavailable for rental.
    /// Triggers a domain event when availability changes.
    /// </summary>
    public void MarkAsUnavailable()
    {
        if (!IsAvailable)
            return;

        IsAvailable = false;
        AddDomainEvent(new CarAvailabilityChangedEvent(this.Id, false));
    }

    /// <summary>
    /// Updates the rating and review count for the car.
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
