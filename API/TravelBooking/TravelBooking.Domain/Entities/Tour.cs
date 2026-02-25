using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Events;
using System.Linq;

namespace TravelBooking.Domain.Entities;

public class Tour : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public int Duration { get; private set; }
    public Money Price { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Difficulty { get; private set; } = "Easy";
    public int MaxGroupSize { get; private set; }
    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

    private readonly List<string> _highlights = [];
    public IReadOnlyCollection<string> Highlights => _highlights.AsReadOnly();

    private readonly List<string> _included = [];
    public IReadOnlyCollection<string> Included => _included.AsReadOnly();

    protected Tour() { }

    public Tour(
        string name,
        string destination,
        int duration,
        Money price,
        string imageUrl,
        string description,
        string difficulty,
        int maxGroupSize,
        List<string> highlights,
        List<string> included)
    {
        if (duration <= 0)
            throw new ArgumentException("Sure pozitif olmalidir.", nameof(duration));
        if (maxGroupSize <= 0)
            throw new ArgumentException("Maksimum grup sayisi pozitif olmalidir.", nameof(maxGroupSize));

        Name = name.Trim();
        Destination = destination.Trim();
        Duration = duration;
        Price = price;
        ImageUrl = imageUrl.Trim();
        Description = description.Trim();
        Difficulty = difficulty.Trim();
        MaxGroupSize = maxGroupSize;
        Rating = 0;
        ReviewCount = 0;

        if (highlights != null)
            _highlights.AddRange(highlights.Select(h => h.Trim()));
        if (included != null)
            _included.AddRange(included.Select(i => i.Trim()));
    }

    /// <summary>
    /// Updates the tour information.
    /// </summary>
    /// <param name="name">The name of the tour.</param>
    /// <param name="destination">The destination of the tour.</param>
    /// <param name="duration">The duration in days.</param>
    /// <param name="price">The price of the tour.</param>
    /// <param name="imageUrl">The URL of the tour's image.</param>
    /// <param name="description">The description of the tour.</param>
    /// <param name="difficulty">The difficulty level.</param>
    /// <param name="maxGroupSize">The maximum group size.</param>
    /// <param name="highlights">The list of tour highlights.</param>
    /// <param name="included">The list of included items.</param>
    /// <exception cref="ArgumentException">Thrown when duration or maxGroupSize is not positive.</exception>
    public void Update(
        string name,
        string destination,
        int duration,
        Money price,
        string imageUrl,
        string description,
        string difficulty,
        int maxGroupSize,
        List<string> highlights,
        List<string> included)
    {
        if (duration <= 0)
            throw new ArgumentException("Sure pozitif olmalidir.", nameof(duration));
        if (maxGroupSize <= 0)
            throw new ArgumentException("Maksimum grup sayisi pozitif olmalidir.", nameof(maxGroupSize));

        var oldPrice = Price;
        var oldDuration = Duration;
        var priceChanged = !oldPrice.Equals(price);
        var durationChanged = oldDuration != duration;

        Name = name.Trim();
        Destination = destination.Trim();
        Duration = duration;
        Price = price;
        ImageUrl = imageUrl.Trim();
        Description = description.Trim();
        Difficulty = difficulty.Trim();
        MaxGroupSize = maxGroupSize;

        _highlights.Clear();
        if (highlights != null)
            _highlights.AddRange(highlights.Select(h => h.Trim()));

        _included.Clear();
        if (included != null)
            _included.AddRange(included.Select(i => i.Trim()));

        if (priceChanged)
            AddDomainEvent(new TourPriceUpdatedEvent(this.Id, oldPrice, price));
        if (durationChanged)
            AddDomainEvent(new TourDurationUpdatedEvent(this.Id, oldDuration, duration));
    }

    /// <summary>
    /// Updates the price of the tour.
    /// Triggers a domain event when the price changes.
    /// </summary>
    /// <param name="newPrice">The new price.</param>
    /// <exception cref="ArgumentNullException">Thrown when newPrice is null.</exception>
    /// <exception cref="ArgumentException">Thrown when price amount is not positive.</exception>
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));
        if (newPrice.Amount <= 0)
            throw new ArgumentException("Fiyat 0'dan buyuk olmalidir.", nameof(newPrice));

        var oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new TourPriceUpdatedEvent(this.Id, oldPrice, newPrice));
    }

    /// <summary>
    /// Updates the duration of the tour.
    /// Triggers a domain event when the duration changes.
    /// </summary>
    /// <param name="newDuration">The new duration in days.</param>
    /// <exception cref="ArgumentException">Thrown when duration is not positive.</exception>
    public void UpdateDuration(int newDuration)
    {
        if (newDuration <= 0)
            throw new ArgumentException("Sure pozitif olmalidir.", nameof(newDuration));

        var oldDuration = Duration;
        Duration = newDuration;

        AddDomainEvent(new TourDurationUpdatedEvent(this.Id, oldDuration, newDuration));
    }

    /// <summary>
    /// Adds a highlight to the tour.
    /// </summary>
    /// <param name="highlight">The highlight to add.</param>
    /// <exception cref="ArgumentException">Thrown when highlight is null or whitespace.</exception>
    public void AddHighlight(string highlight)
    {
        if (string.IsNullOrWhiteSpace(highlight))
            throw new ArgumentException("Highlight bos olamaz.", nameof(highlight));

        var trimmed = highlight.Trim();
        if (!_highlights.Contains(trimmed))
            _highlights.Add(trimmed);
    }

    /// <summary>
    /// Removes a highlight from the tour.
    /// </summary>
    /// <param name="highlight">The highlight to remove.</param>
    public void RemoveHighlight(string highlight)
    {
        if (string.IsNullOrWhiteSpace(highlight))
            return;

        _highlights.Remove(highlight.Trim());
    }

    /// <summary>
    /// Adds an included item to the tour.
    /// </summary>
    /// <param name="included">The included item to add.</param>
    /// <exception cref="ArgumentException">Thrown when included is null or whitespace.</exception>
    public void AddIncluded(string included)
    {
        if (string.IsNullOrWhiteSpace(included))
            throw new ArgumentException("Included bos olamaz.", nameof(included));

        var trimmed = included.Trim();
        if (!_included.Contains(trimmed))
            _included.Add(trimmed);
    }

    /// <summary>
    /// Removes an included item from the tour.
    /// </summary>
    /// <param name="included">The included item to remove.</param>
    public void RemoveIncluded(string included)
    {
        if (string.IsNullOrWhiteSpace(included))
            return;

        _included.Remove(included.Trim());
    }

    /// <summary>
    /// Updates the rating and review count for the tour.
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
