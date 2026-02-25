using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.DTOs.Flights;

public class CreateFlightDto
{
    [Required(ErrorMessage = "Ucus numarasi gereklidir")]
    [StringLength(20, ErrorMessage = "Ucus numarasi en fazla 20 karakter olabilir")]
    public string FlightNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Havayolu adi gereklidir")]
    [StringLength(100, ErrorMessage = "Havayolu adi en fazla 100 karakter olabilir")]
    public string AirlineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kalkis havalimani gereklidir")]
    public Guid DepartureAirportId { get; set; }

    [Required(ErrorMessage = "Varis havalimani gereklidir")]
    public Guid ArrivalAirportId { get; set; }

    [Required(ErrorMessage = "Kalkis zamani gereklidir")]
    public DateTime ScheduledDeparture { get; set; } = DateTime.Now.AddDays(1);

    [Required(ErrorMessage = "Varis zamani gereklidir")]
    public DateTime ScheduledArrival { get; set; } = DateTime.Now.AddDays(1).AddHours(2);

    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan buyuk olmalidir")]
    public decimal BasePriceAmount { get; set; }

    [Required(ErrorMessage = "Para birimi gereklidir")]
    public string Currency { get; set; } = "TRY";

    [Required(ErrorMessage = "Toplam koltuk sayisi gereklidir")]
    [Range(1, 1000, ErrorMessage = "Koltuk sayisi 1-1000 arasinda olmalidir")]
    public int TotalSeats { get; set; } = 180;

    [Required(ErrorMessage = "Ucus tipi gereklidir")]
    public string FlightType { get; set; } = "Direct";

    [Required(ErrorMessage = "Ucus bolgesi gereklidir")]
    public string FlightRegion { get; set; } = "Domestic";
}
