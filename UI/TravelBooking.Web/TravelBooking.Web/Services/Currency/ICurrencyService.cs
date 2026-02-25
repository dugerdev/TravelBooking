namespace TravelBooking.Web.Services.Currency;

public interface ICurrencyService
{
    /// <summary>
    /// Converts an amount from one currency to another
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrency">Source currency code (e.g., TRY)</param>
    /// <param name="toCurrency">Target currency code (e.g., USD)</param>
    /// <returns>Converted amount</returns>
    decimal ConvertPrice(decimal amount, string fromCurrency, string toCurrency);

    /// <summary>
    /// Gets the exchange rate between two currencies
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <returns>Exchange rate</returns>
    decimal GetExchangeRate(string fromCurrency, string toCurrency);

    /// <summary>
    /// Formats a price with the appropriate currency symbol
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <param name="currencyCode">Currency code</param>
    /// <returns>Formatted price string</returns>
    string FormatPrice(decimal amount, string currencyCode);

    /// <summary>
    /// Gets the currency symbol for a given currency code
    /// </summary>
    /// <param name="currencyCode">Currency code</param>
    /// <returns>Currency symbol</returns>
    string GetCurrencySymbol(string currencyCode);
}
