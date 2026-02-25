using TravelBooking.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TravelBooking.Web.Filters;

public class LocalizationActionFilter : IActionFilter
{
    private readonly ICookieHelper _cookieHelper;

    public LocalizationActionFilter(ICookieHelper cookieHelper)
    {
        _cookieHelper = cookieHelper;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.SelectedCurrency = _cookieHelper.GetCurrency();
            controller.ViewBag.SelectedLanguage = _cookieHelper.GetLanguage();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed
    }
}
