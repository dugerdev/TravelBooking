using TravelBooking.Web.Services.Currency;

namespace TravelBooking.Web.Helpers;

public interface IViewCurrencyHelper
{
    /// <summary>
    /// Converts price from TRY to the user's selected currency
    /// </summary>
    decimal ConvertFromBase(decimal amount, string targetCurrency);

    /// <summary>
    /// Converts amount from one currency to another
    /// </summary>
    decimal ConvertPrice(decimal amount, string fromCurrency, string toCurrency);

    /// <summary>
    /// Formats a price in TRY for display in selected currency (converts then formats)
    /// </summary>
    string FormatPriceInCurrency(decimal amountInTry, string displayCurrency);

    /// <summary>
    /// Formats price with the appropriate currency symbol
    /// </summary>
    string FormatPrice(decimal amount, string currencyCode);

    /// <summary>
    /// Formats price with decimals (e.g. 1,234.56)
    /// </summary>
    string FormatPriceWithDecimals(decimal amount, string currencyCode);

    /// <summary>
    /// Formats a price in TRY with decimals in selected currency
    /// </summary>
    string FormatPriceWithDecimalsInCurrency(decimal amountInTry, string displayCurrency);

    /// <summary>
    /// Gets the currency symbol for a given currency code
    /// </summary>
    string GetSymbol(string currencyCode);

    /// <summary>
    /// Alias for GetSymbol for view compatibility
    /// </summary>
    string GetCurrencySymbol(string currencyCode);

    /// <summary>
    /// Converts and formats a price from product's currency to selected currency
    /// </summary>
    string FormatPriceInSelectedCurrency(decimal amount, string productCurrency, string selectedCurrency);

    /// <summary>
    /// Converts and formats a price with decimals from product's currency to selected currency
    /// </summary>
    string FormatPriceWithDecimalsInSelectedCurrency(decimal amount, string productCurrency, string selectedCurrency);
}

public class ViewCurrencyHelper(ICurrencyService currencyService) : IViewCurrencyHelper
{

    public decimal ConvertFromBase(decimal amount, string targetCurrency)
    {
        return currencyService.ConvertPrice(amount, "TRY", targetCurrency);
    }

    public decimal ConvertPrice(decimal amount, string fromCurrency, string toCurrency)
    {
        return currencyService.ConvertPrice(amount, fromCurrency ?? "TRY", toCurrency ?? "TRY");
    }

    public string FormatPriceInCurrency(decimal amountInTry, string displayCurrency)
    {
        var converted = ConvertFromBase(amountInTry, displayCurrency ?? "TRY");
        return currencyService.FormatPrice(converted, displayCurrency ?? "TRY");
    }

    public string FormatPrice(decimal amount, string currencyCode)
    {
        return currencyService.FormatPrice(amount, currencyCode);
    }

    public string FormatPriceWithDecimals(decimal amount, string currencyCode)
    {
        return currencyService.FormatPrice(amount, currencyCode);
    }

    public string FormatPriceWithDecimalsInCurrency(decimal amountInTry, string displayCurrency)
    {
        var converted = ConvertFromBase(amountInTry, displayCurrency ?? "TRY");
        return currencyService.FormatPrice(converted, displayCurrency ?? "TRY");
    }

    public string GetSymbol(string currencyCode)
    {
        return currencyService.GetCurrencySymbol(currencyCode ?? "TRY");
    }

    public string GetCurrencySymbol(string currencyCode)
    {
        return GetSymbol(currencyCode);
    }

    public string FormatPriceInSelectedCurrency(decimal amount, string productCurrency, string selectedCurrency)
    {
        var converted = currencyService.ConvertPrice(amount, productCurrency ?? "TRY", selectedCurrency ?? "TRY");
        return currencyService.FormatPrice(converted, selectedCurrency ?? "TRY");
    }

    public string FormatPriceWithDecimalsInSelectedCurrency(decimal amount, string productCurrency, string selectedCurrency)
    {
        var converted = currencyService.ConvertPrice(amount, productCurrency ?? "TRY", selectedCurrency ?? "TRY");
        return currencyService.FormatPrice(converted, selectedCurrency ?? "TRY");
    }
}
