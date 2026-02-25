using System.Globalization;

namespace TravelBooking.Web.Services.Currency;

public class CurrencyService : ICurrencyService
{
    // Exchange rates relative to TRY (Turkish Lira as base)
    // In a real application, these would be fetched from an API (e.g., exchangerate-api.com, fixer.io)
    private readonly Dictionary<string, decimal> _exchangeRates = new()
    {
        { "TRY", 1.0m },       // Turk Lirasi (baz)
        { "USD", 0.0230m },    // 1 TRY ≈ 0.023 USD (1 USD ≈ ~43.5 TRY)
        { "EUR", 0.0190m },    // 1 TRY ≈ 0.019 EUR (1 EUR ≈ ~52.0 TRY)
        { "GBP", 0.0167m },    // 1 TRY ≈ 0.0167 GBP (1 GBP ≈ ~59.6 TRY)
        { "JPY", 2.50m }       // 1 TRY ≈ 2.50 JPY
    };

    private readonly Dictionary<string, string> _currencySymbols = new()
    {
        { "TRY", "₺" },
        { "USD", "$" },
        { "EUR", "€" },
        { "GBP", "£" },
        { "JPY", "¥" }
    };

    public decimal ConvertPrice(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            return amount;

        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        if (fromCurrency == toCurrency)
            return amount;

        if (!_exchangeRates.TryGetValue(fromCurrency, out var fromRate) || !_exchangeRates.TryGetValue(toCurrency, out var toRate))
            return amount;

        // Convert from source currency to TRY (base)
        var amountInTRY = amount / fromRate;

        // Convert from TRY to target currency
        var convertedAmount = amountInTRY * toRate;

        return Math.Round(convertedAmount, 2);
    }

    public decimal GetExchangeRate(string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            return 1.0m;

        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        if (fromCurrency == toCurrency)
            return 1.0m;

        if (!_exchangeRates.ContainsKey(fromCurrency) || !_exchangeRates.ContainsKey(toCurrency))
            return 1.0m;

        // Calculate exchange rate
        return _exchangeRates[toCurrency] / _exchangeRates[fromCurrency];
    }

    public string FormatPrice(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            currencyCode = "TRY";

        currencyCode = currencyCode.ToUpperInvariant();

        var symbol = GetCurrencySymbol(currencyCode);
        
        // Format with appropriate decimal places
        var formattedAmount = amount.ToString("N2", CultureInfo.InvariantCulture);

        // Different currencies have different formatting conventions
        return currencyCode switch
        {
            "USD" or "GBP" => $"{symbol}{formattedAmount}",
            "EUR" => $"{formattedAmount} {symbol}",
            "JPY" => $"{symbol}{Math.Round(amount, 0):N0}",  // JPY doesn't use decimal places
            "TRY" => $"{formattedAmount} {symbol}",
            _ => $"{formattedAmount} {symbol}"
        };
    }

    public string GetCurrencySymbol(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return "₺";

        currencyCode = currencyCode.ToUpperInvariant();

        return _currencySymbols.TryGetValue(currencyCode, out var symbol) ? symbol : currencyCode;
    }
}
