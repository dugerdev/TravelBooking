namespace TravelBooking.Web.ViewModels.Hotels;

public class HotelViewModel
{
    public int Id { get; set; }
    public Guid RawId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = "/assets/img/hotel-default.jpg";
    public string Description { get; set; } = string.Empty;
    public List<string> Amenities { get; set; } = [];
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool HasFreeWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasPool { get; set; }
    public bool HasRestaurant { get; set; }
}

public class HotelListingViewModel
{
    public List<HotelViewModel> Hotels { get; set; } = new();
    public string? SearchCity { get; set; }
    public int? MinStarRating { get; set; }
    public decimal? MaxPricePerNight { get; set; }
}

public class HotelDetailViewModel
{
    public HotelViewModel Hotel { get; set; } = new();
    public List<RoomViewModel> Rooms { get; set; } = new();
    public List<HotelReviewViewModel> Reviews { get; set; } = new();
}

public class RoomViewModel
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int MaxGuests { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = [];
}

public class HotelReviewViewModel
{
    public string GuestName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
