using TravelBooking.Web.ViewModels.Admin;

namespace TravelBooking.Web.Services.Settings;

public interface ISiteSettingsService
{
    Task<SettingsViewModel> GetAsync(CancellationToken ct = default);
    Task SaveAsync(SettingsViewModel model, CancellationToken ct = default);
}
