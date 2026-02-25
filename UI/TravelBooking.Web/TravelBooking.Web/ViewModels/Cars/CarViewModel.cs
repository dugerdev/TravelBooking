namespace TravelBooking.Web.ViewModels.Cars;

public class CarViewModel
{
    public int Id { get; set; }
    public Guid RawId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Economy, Compact, SUV, Luxury
    public int Year { get; set; }
    public string FuelType { get; set; } = "Gasoline";
    public string Transmission { get; set; } = "Automatic";
    public int Seats { get; set; }
    public int Doors { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = "/assets/img/car-default.jpg";
    public string Location { get; set; } = string.Empty;
    public bool HasAirConditioning { get; set; }
    public bool HasGPS { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
}

public class CarListingViewModel
{
    public List<CarViewModel> Cars { get; set; } = [];
    public string? SearchLocation { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Category { get; set; }
}

public class CarDetailViewModel
{
    public CarViewModel Car { get; set; } = new();
    public List<string> Features { get; set; } = [];
    public List<string> IncludedInPrice { get; set; } = [];
    public List<CarReviewViewModel> Reviews { get; set; } = [];
    public DateTime PickupDate { get; set; } = DateTime.Now.AddDays(1);
    public DateTime ReturnDate { get; set; } = DateTime.Now.AddDays(3);
    public string PickupLocation { get; set; } = string.Empty;
    public string ReturnLocation { get; set; } = string.Empty;
}

public class CarReviewViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
