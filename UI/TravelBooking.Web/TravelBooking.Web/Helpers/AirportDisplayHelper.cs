namespace TravelBooking.Web.Helpers;

/// <summary>
/// IATA kodundan sehir adi ve guzergah metni uretmek icin fallback sozluk.
/// API'den City gelmediginde kullanilir.
/// </summary>
public static class AirportDisplayHelper
{
    private static readonly IReadOnlyDictionary<string, string> IataToCity = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "IST", "Istanbul" }, { "SAW", "Istanbul" }, { "ADB", "Izmir" }, { "AYT", "Antalya" }, { "ESB", "Ankara" },
        { "JFK", "New York" }, { "LAX", "Los Angeles" }, { "ORD", "Chicago" }, { "MIA", "Miami" }, { "SFO", "San Francisco" },
        { "CDG", "Paris" }, { "LHR", "Londra" }, { "LGW", "Londra" }, { "AMS", "Amsterdam" }, { "FRA", "Frankfurt" },
        { "MUC", "Munih" }, { "MAD", "Madrid" }, { "BCN", "Barcelona" }, { "FCO", "Roma" }, { "MXP", "Milano" },
        { "ATH", "Atina" }, { "LIS", "Lizbon" }, { "DUB", "Dublin" }, { "BRU", "Bruksel" }, { "VIE", "Viyana" },
        { "ZRH", "Zurih" }, { "CPH", "Kopenhag" }, { "ARN", "Stockholm" }, { "OSL", "Oslo" }, { "DXB", "Dubai" },
        { "AUH", "Abu Dabi" }, { "DOH", "Doha" }, { "CAI", "Kahire" }, { "TLV", "Tel Aviv" }, { "SIN", "Singapur" },
        { "BKK", "Bangkok" }, { "HKG", "Hong Kong" }, { "ICN", "Seul" }, { "NRT", "Tokyo" }, { "HND", "Tokyo" },
        { "SYD", "Sydney" }, { "MEL", "Melbourne" }, { "YYZ", "Toronto" }, { "MEX", "Meksiko" }, { "GRU", "São Paulo" },
        { "SVO", "Moskova" }, { "LED", "St. Petersburg" }
    };

    /// <summary>
    /// IATA koduna gore sehir adi dondurur. Bilinmiyorsa IATA kodunu dondurur.
    /// </summary>
    public static string GetCityName(string? iata)
    {
        if (string.IsNullOrWhiteSpace(iata)) return "-";
        var key = iata.Trim().ToUpperInvariant();
        return IataToCity.TryGetValue(key, out var city) ? city : iata;
    }

    /// <summary>
    /// "IST → JFK" gibi bir ozeti "Istanbul (IST) → New York (JFK)" formatina cevirir.
    /// </summary>
    public static string GetRouteDisplay(string? reservationSummary)
    {
        if (string.IsNullOrWhiteSpace(reservationSummary)) return "-";
        var s = reservationSummary.Trim();
        if (!s.Contains("→"))
            return s;
        var parts = s.Split(new[] { "→", "->" }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return s;
        var dep = parts[0].Trim();
        var arr = parts[1].Trim();
        var depCity = GetCityName(dep);
        var arrCity = GetCityName(arr);
        return $"{depCity} ({dep}) → {arrCity} ({arr})";
    }
}
