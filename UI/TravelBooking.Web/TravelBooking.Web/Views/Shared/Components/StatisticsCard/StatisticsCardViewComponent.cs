using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels;

namespace TravelBooking.Web.Views.Shared.Components.StatisticsCard;

public class StatisticsCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string title, string value, string icon = "fas fa-fw fa-circle", string borderColor = "primary")
    {
        return View(new StatCardViewModel { Title = title, Value = value, Icon = icon, BorderColor = borderColor });
    }
}
