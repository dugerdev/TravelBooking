namespace TravelBooking.Web.ViewModels.Tours;

public class TourViewModel
{
    public int Id { get; set; }
    public Guid RawId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Duration { get; set; } // in days
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = "/assets/img/tour-default.jpg";
    public string Description { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = [];
    public List<string> Included { get; set; } = [];
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string Difficulty { get; set; } = "Easy"; // Easy, Moderate, Challenging
    public int MaxGroupSize { get; set; }
}

public class TourPackageViewModel
{
    public int Id { get; set; }
    public Guid RawId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int Duration { get; set; }
    public List<string> Destinations { get; set; } = new();
    public List<string> Included { get; set; } = new();
    public string ImageUrl { get; set; } = "/assets/img/package-default.jpg";
}

public class TourDetailViewModel
{
    public TourViewModel Tour { get; set; } = new();
    public List<string> Itinerary { get; set; } = [];
    public List<TourReviewViewModel> Reviews { get; set; } = [];
    public List<DateTime> AvailableDates { get; set; } = [];
}

public class TourReviewViewModel
{
    public string TravelerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class TourPackagesViewModel
{
    public List<TourPackageViewModel> Packages { get; set; } = [];
    public string? SearchDestination { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
