using TravelBooking.Web.Models;

namespace TravelBooking.Web.ViewModels;

public class SearchResultViewModel
{
    public string SearchQuery { get; set; } = string.Empty;

    // Her kategoriden gelecek sonuc listeleri:
    public List<Models.Flight> Flights { get; set; } = [];
    public List<Models.News> NewsItems { get; set; } = [];
    public List<Models.Car> Cars { get; set; } = [];
    public List<Models.Tours> ToursItems { get; set; } = [];

}