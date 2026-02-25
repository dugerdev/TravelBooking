using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.ViewModels.Admin;
using TravelBooking.Web.Services.Settings;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISiteSettingsService _settingsService;

    public SettingsController(ISiteSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var model = await _settingsService.GetAsync(ct);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(SettingsViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        await _settingsService.SaveAsync(model, ct);
        TempData["Success"] = "Ayarlar başarıyla kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
