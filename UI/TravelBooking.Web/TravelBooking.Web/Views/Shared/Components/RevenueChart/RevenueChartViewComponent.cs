using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels;

namespace TravelBooking.Web.Views.Shared.Components.RevenueChart;

public class RevenueChartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Dictionary<string, decimal>? data, string title = "Gelir Özeti (Son 12 Ay)", string canvasId = "revenueChart")
    {
        return View(new RevenueChartViewModel { Data = data ?? new Dictionary<string, decimal>(), Title = title, CanvasId = canvasId });
    }
}
