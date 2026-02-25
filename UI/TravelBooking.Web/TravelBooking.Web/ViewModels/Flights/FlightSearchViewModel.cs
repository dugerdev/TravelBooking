using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Flights;

public class FlightSearchViewModel
{
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public string? DepartureDate { get; set; }
    public string? ReturnDate { get; set; }
    public int AdultCount { get; set; } = 1;
    public int ChildCount { get; set; }
    public int InfantCount { get; set; }
    public bool DirectFlight { get; set; }
    public string? Way { get; set; } = "one-way";
    public string? CabinClass { get; set; } = "Economy";
}
