namespace TravelBooking.Web.Configuration;

public class TravelBookingApiOptions
{
    public const string SectionName = "TravelBookingApi";
    public string BaseUrl { get; set; } = "https://localhost:7283";
    public int TimeoutSeconds { get; set; } = 30;
}
