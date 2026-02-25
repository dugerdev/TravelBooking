using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels;

namespace TravelBooking.Web.Views.Shared.Components.ReservationPieChart;

public class ReservationPieChartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int pending, int confirmed, int cancelled, int completed, string title = "Rezervasyon Durumları", string canvasId = "reservationsPieChart")
    {
        return View(new ReservationPieChartViewModel
        {
            Pending = pending,
            Confirmed = confirmed,
            Cancelled = cancelled,
            Completed = completed,
            Title = title,
            CanvasId = canvasId
        });
    }
}
