namespace TravelBooking.Web.ViewModels;

public class ReservationPieChartViewModel
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int Completed { get; set; }
    public string Title { get; set; } = "Rezervasyon Durumları";
    public string CanvasId { get; set; } = "reservationsPieChart";
}
