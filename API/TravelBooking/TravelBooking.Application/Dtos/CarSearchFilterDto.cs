namespace TravelBooking.Application.Dtos;

public class CarSearchFilterDto
{
    // Basic search
    public string? Location { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    
    // Price range
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Transmission
    public List<string>? Transmissions { get; set; } // Automatic, Manual
    
    // Fuel type
    public List<string>? FuelTypes { get; set; } // Electric, Hybrid, Petrol, Diesel
    
    // Category
    public List<string>? Categories { get; set; } // Economy, Luxury, SUV, Compact, etc.
    
    // Brand/Supplier
    public List<string>? Brands { get; set; }
    public List<string>? Suppliers { get; set; }
    
    // Capacity
    public int? MinSeats { get; set; }
    public int? MinDoors { get; set; }
    
    // Policies
    public List<string>? MileagePolicies { get; set; } // Unlimited, Limited
    public List<string>? FuelPolicies { get; set; } // Full to Full, Same to Same
    public List<string>? PickupLocationTypes { get; set; } // In Terminal, Shuttle, Meet & Greet
    
    // Features
    public bool? HasAirConditioning { get; set; }
    public bool? HasGPS { get; set; }
    
    // Rating
    public double? MinRating { get; set; }
    
    // Sorting
    public string? SortBy { get; set; } // price_asc, price_desc, rating_desc, etc.
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
