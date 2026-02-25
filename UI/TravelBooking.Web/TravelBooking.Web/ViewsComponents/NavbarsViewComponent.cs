using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Auth;

namespace TravelBooking.Web.ViewsComponents;

public class NavbarsViewComponent : ViewComponent
{
    private readonly IAuthService _authService;

    public NavbarsViewComponent(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        await Task.CompletedTask;
        
        var model = new NavbarViewModel
        {
            IsAuthenticated = _authService.IsAuthenticated(),
            UserName = _authService.GetCurrentUserName(),
            Roles = _authService.GetCurrentUserRoles()
        };

        return View(model);
    }
}

public class NavbarViewModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsAdmin => Roles.Contains("Admin");
}
