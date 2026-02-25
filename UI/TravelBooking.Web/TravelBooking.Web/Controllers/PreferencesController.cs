using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Helpers;

namespace TravelBooking.Web.Controllers;

public class PreferencesController : Controller
{
    private readonly ICookieHelper _cookieHelper;

    public PreferencesController(ICookieHelper cookieHelper)
    {
        _cookieHelper = cookieHelper;
    }

    [HttpPost]
    public IActionResult SetCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest(new { success = false, message = "Para birimi gereklidir." });

        var validCurrencies = new[] { "TRY", "USD", "EUR", "GBP", "JPY" };
        if (!validCurrencies.Contains(currency.ToUpperInvariant()))
            return BadRequest(new { success = false, message = "Invalid currency." });

        _cookieHelper.SetCurrency(currency.ToUpperInvariant());
        return Ok(new { success = true, currency = currency.ToUpperInvariant() });
    }

    [HttpPost]
    public IActionResult SetLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return BadRequest(new { success = false, message = "Language is required." });

        var validLanguages = new[] { "tr", "en" };
        if (!validLanguages.Contains(language.ToLowerInvariant()))
            return BadRequest(new { success = false, message = "Invalid language." });

        _cookieHelper.SetLanguage(language.ToLowerInvariant());
        return Ok(new { success = true, language = language.ToLowerInvariant() });
    }
}
