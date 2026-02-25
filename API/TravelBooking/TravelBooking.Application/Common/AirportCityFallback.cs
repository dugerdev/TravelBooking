namespace TravelBooking.Application.Common;

/// <summary>
/// IATA kodundan sehir adi fallback. API'de Airport.City null/bos oldugunda DTO'da sehir gostermek icin kullanilir.
/// </summary>
public static class AirportCityFallback
{
    private static readonly IReadOnlyDictionary<string, string> IataToCity = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "IST", "Istanbul" }, { "SAW", "Istanbul" }, { "ADB", "Izmir" }, { "AYT", "Antalya" }, { "ESB", "Ankara" },
        { "JFK", "New York" }, { "LAX", "Los Angeles" }, { "ORD", "Chicago" }, { "MIA", "Miami" }, { "SFO", "San Francisco" },
        { "CDG", "Paris" }, { "LHR", "London" }, { "LGW", "London" }, { "AMS", "Amsterdam" }, { "FRA", "Frankfurt" },
        { "MUC", "Munich" }, { "MAD", "Madrid" }, { "BCN", "Barcelona" }, { "FCO", "Rome" }, { "MXP", "Milan" },
        { "ATH", "Athens" }, { "LIS", "Lisbon" }, { "DUB", "Dublin" }, { "BRU", "Brussels" }, { "VIE", "Vienna" },
        { "ZRH", "Zurich" }, { "CPH", "Copenhagen" }, { "ARN", "Stockholm" }, { "OSL", "Oslo" }, { "DXB", "Dubai" },
        { "AUH", "Abu Dhabi" }, { "DOH", "Doha" }, { "CAI", "Cairo" }, { "TLV", "Tel Aviv" }, { "SIN", "Singapore" },
        { "BKK", "Bangkok" }, { "HKG", "Hong Kong" }, { "ICN", "Seoul" }, { "NRT", "Tokyo" }, { "HND", "Tokyo" },
        { "SYD", "Sydney" }, { "MEL", "Melbourne" }, { "YYZ", "Toronto" }, { "MEX", "Mexico City" }, { "GRU", "São Paulo" },
        { "SVO", "Moscow" }, { "LED", "St Petersburg" }
    };

    /// <summary>
    /// IATA koduna gore sehir adi dondurur. Bilinmiyorsa null doner (mevcut City kullanilir veya bos kalir).
    /// </summary>
    public static string? GetCity(string? iata)
    {
        if (string.IsNullOrWhiteSpace(iata)) return null;
        var key = iata.Trim().ToUpperInvariant();
        return IataToCity.TryGetValue(key, out var city) ? city : null;
    }

    /// <summary>
    /// Airport.City doluysa onu dondurur, degilse IATA'dan fallback sehir adini dondurur.
    /// </summary>
    public static string ResolveCity(string? city, string? iataCode)
    {
        if (!string.IsNullOrWhiteSpace(city)) return city;
        return GetCity(iataCode) ?? string.Empty;
    }
}
