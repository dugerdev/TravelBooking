using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
   

}
