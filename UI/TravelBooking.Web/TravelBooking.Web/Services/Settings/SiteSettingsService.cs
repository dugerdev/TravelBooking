using System.Text.Json;
using TravelBooking.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Hosting;

namespace TravelBooking.Web.Services.Settings;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public SiteSettingsService(IWebHostEnvironment env)
    {
        _env = env;
        var appDataPath = Path.Combine(_env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _filePath = Path.Combine(appDataPath, "site-settings.json");
    }

    public async Task<SettingsViewModel> GetAsync(CancellationToken ct = default)
    {
        if (!System.IO.File.Exists(_filePath))
            return GetDefaults();

        try
        {
            var json = await System.IO.File.ReadAllTextAsync(_filePath, ct);
            var model = JsonSerializer.Deserialize<SettingsViewModel>(json, JsonOptions);
            return model ?? GetDefaults();
        }
        catch
        {
            return GetDefaults();
        }
    }

    public async Task SaveAsync(SettingsViewModel model, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(model, JsonOptions);
        await System.IO.File.WriteAllTextAsync(_filePath, json, ct);
    }

    private static SettingsViewModel GetDefaults()
    {
        return new SettingsViewModel
        {
            SiteName = "TravelBooking",
            SiteEmail = "info@gocebe.com",
            SupportEmail = "support@gocebe.com",
            SupportPhone = "+90 212 555 0000",
            MaintenanceMode = false,
            AllowRegistration = true,
            EmailVerificationRequired = true,
            DefaultCurrency = "TRY",
            DefaultLanguage = "tr-TR"
        };
    }
}
