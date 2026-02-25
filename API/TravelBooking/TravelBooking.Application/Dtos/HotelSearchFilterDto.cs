namespace TravelBooking.Application.Dtos;

public class HotelSearchFilterDto
{
    // Basic search
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // Price range
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Star rating
    public int? MinStarRating { get; set; }
    public List<int>? StarRatings { get; set; }
    
    // Property type
    public List<string>? PropertyTypes { get; set; }
    
    // Distance
    public double? MaxDistanceFromCenter { get; set; }
    
    // Review score
    public double? MinReviewScore { get; set; }
    
    // Sustainability
    public int? MinSustainabilityLevel { get; set; }
    
    // Brand/Chain
    public List<string>? Brands { get; set; }
    
    // Neighbourhood
    public List<string>? Neighbourhoods { get; set; }
    
    // Facilities
    public bool? HasFreeWifi { get; set; }
    public bool? HasParking { get; set; }
    public bool? HasPool { get; set; }
    public bool? HasRestaurant { get; set; }
    public bool? HasAirConditioning { get; set; }
    public bool? HasFitnessCenter { get; set; }
    public bool? HasSpa { get; set; }
    
    // Meal options
    public bool? HasBreakfast { get; set; }
    
    // Booking options
    public bool? HasFreeCancellation { get; set; }
    public bool? NoPrepaymentNeeded { get; set; }
    
    // Accessibility
    public bool? HasAccessibilityFeatures { get; set; }
    
    // Sorting
    public string? SortBy { get; set; } // price_asc, price_desc, rating_desc, distance_asc, etc.
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
