using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels;

namespace TravelBooking.Web.Views.Shared.Components.Button;

public class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string text, string variant = "primary", string? icon = null, string? url = null, string size = "", string cssClass = "", bool submit = false, bool disabled = false)
    {
        return View(new ButtonViewModel
        {
            Text = text,
            Variant = variant,
            Icon = icon ?? "",
            Url = url,
            Size = size,
            CssClass = cssClass,
            Submit = submit,
            Disabled = disabled
        });
    }
}
