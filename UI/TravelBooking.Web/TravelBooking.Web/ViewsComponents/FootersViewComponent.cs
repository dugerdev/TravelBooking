using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Auth;

namespace TravelBooking.Web.ViewsComponents;

public class FootersViewComponent : ViewComponent
{
    private readonly IAuthService _authService;

    public FootersViewComponent(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        await Task.CompletedTask;

        var model = new FooterViewModel
        {
            IsAuthenticated = _authService.IsAuthenticated(),
            CurrentYear = DateTime.Now.Year
        };

        return View(model);
    }
}

public class FooterViewModel
{
    public bool IsAuthenticated { get; set; }
    public int CurrentYear { get; set; }
}
