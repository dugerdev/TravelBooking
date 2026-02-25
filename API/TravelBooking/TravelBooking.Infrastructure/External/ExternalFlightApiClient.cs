using System.Text.Json;
using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Dtos.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Infrastructure.External;

/// <summary>AeroDataBox (RapidAPI) FIDS endpoint'leri ile kalkis/varis IATA ve tarih uzerinden ucus aramasi.</summary>
public class ExternalFlightApiClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ExternalFlightApiClient> logger) : IExternalFlightApiClient
{
    private static readonly string[] TurkishAirports = { "IST", "SAW", "ESB", "ADB", "AYT", "DLM", "BJV", "GZT", "ADA", "TZX" };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("AeroDataBox");

    public async Task<IEnumerable<ExternalFlightDto>> GetFlightsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogWarning("GetFlightsAsync AeroDataBox ile desteklenmiyor (havalimani/tarih gerekir). Bos dizi donuluyor.");
        return await Task.FromResult(Enumerable.Empty<ExternalFlightDto>()).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ExternalFlightDto>> GetFlightsFilteredAsync(string depIata, string arrIata, DateTime flightDate, CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["AeroDataBox:RapidAPIKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("AeroDataBox:RapidAPIKey configuration is missing or empty");
            throw new InvalidOperationException("AeroDataBox:RapidAPIKey is required. Please check appsettings (or AeroDataBox:RapidAPIKey in User Secrets).");
        }

        if (_httpClient.BaseAddress == null)
        {
            logger.LogError("HttpClient BaseAddress is null. Check AeroDataBox:BaseUrl.");
            throw new InvalidOperationException("AeroDataBox BaseAddress is not configured.");
        }

        var dateStr = flightDate.ToString("yyyy-MM-dd");
        var all = new List<ExternalFlightDto>();

        // FIDS: fromLocal–toLocal arasi en fazla 12 saat. Gunu iki pencerede cekiyoruz.
        var windows = new[]
        {
            ($"{dateStr}T00:00", $"{dateStr}T12:00"),
            ($"{dateStr}T12:00", $"{dateStr}T23:59")
        };

        foreach (var (fromLocal, toLocal) in windows)
        {
            var path = $"flights/airports/iata/{depIata.ToUpperInvariant()}/{fromLocal}/{toLocal}?direction=Departure&withLeg=true";
            logger.LogDebug("AeroDataBox FIDS: {Path}", path);

            try
            {
                var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogWarning("AeroDataBox FIDS {StatusCode}: {Body}", response.StatusCode, body);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var fids = JsonSerializer.Deserialize<AeroDataBoxFidsResponse>(json, JsonOptions);

                var depList = fids?.Departures ?? new List<AeroDataBoxFlight>();
                foreach (var f in depList)
                {
                    var arrIataActual = f.Arrival?.Airport?.Iata?.Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(arrIataActual) || arrIataActual != arrIata.Trim().ToUpperInvariant())
                        continue;

                    var dto = MapToExternalFlightDto(f, depIata, arrIata);
                    if (dto != null)
                        all.Add(dto);
                }
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(ex, "AeroDataBox FIDS timeout. AeroDataBox:TimeoutSeconds artirilabilir.");
                throw new TimeoutException("AeroDataBox istegi zaman asimina ugradi.", ex);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AeroDataBox FIDS penceresi atlandi: {From}–{To}", fromLocal, toLocal);
            }
        }

        var ordered = all
            .GroupBy(x => x.ExternalFlightId)
            .Select(g => g.First())
            .OrderBy(x => x.ScheduledDeparture)
            .ToList();

        logger.LogInformation("AeroDataBox GetFlightsFilteredAsync: dep={Dep}, arr={Arr}, date={Date} → {Count} ucus.",
            depIata, arrIata, dateStr, ordered.Count);
        return ordered;
    }

    public async Task<IEnumerable<ExternalFlightDto>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("GetFlightsByDateRangeAsync AeroDataBox ile desteklenmiyor (havalimani kodu gerekir). Bos dizi donuluyor.");
        return await Task.FromResult(Enumerable.Empty<ExternalFlightDto>()).ConfigureAwait(false);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private ExternalFlightDto? MapToExternalFlightDto(AeroDataBoxFlight f, string depIata, string arrIata)
    {
        if (string.IsNullOrWhiteSpace(f.Number))
            return null;

        var dep = f.Departure;
        var arr = f.Arrival;
        var depAirport = dep?.Airport;
        var arrAirport = arr?.Airport;

        var scheduledDep = ParseDt(dep?.ScheduledTime?.Utc ?? dep?.ScheduledTime?.Local) ?? DateTime.UtcNow;
        var scheduledArr = ParseDt(arr?.ScheduledTime?.Utc ?? arr?.ScheduledTime?.Local);
        if (scheduledArr == null || scheduledArr <= scheduledDep)
            scheduledArr = scheduledDep.AddHours(1);

        var depIataFinal = depAirport?.Iata?.Trim().ToUpperInvariant() ?? depIata.Trim().ToUpperInvariant();
        var arrIataFinal = arrAirport?.Iata?.Trim().ToUpperInvariant() ?? arrIata.Trim().ToUpperInvariant();

        var externalId = $"{f.Number}_{scheduledDep:yyyyMMddHHmm}";

        var flightRegion = DetermineFlightRegion(depIataFinal, arrIataFinal);
        var calculatedPrice = CalculateRealisticPrice(depIataFinal, arrIataFinal, scheduledDep, flightRegion);
        
        return new ExternalFlightDto
        {
            ExternalFlightId = externalId,
            FlightNumber = f.Number.Trim(),
            AirlineName = f.Airline?.Name?.Trim() ?? "Unknown",
            DepartureAirportIATA = depIataFinal,
            DepartureAirportName = depAirport?.Name ?? depAirport?.MunicipalityName,
            ArrivalAirportIATA = arrIataFinal,
            ArrivalAirportName = arrAirport?.Name ?? arrAirport?.MunicipalityName,
            ScheduledDeparture = scheduledDep,
            ScheduledArrival = scheduledArr.Value,
            BasePriceAmount = calculatedPrice,
            Currency = "TRY",
            TotalSeats = 200,
            AvailableSeats = 200,
            FlightType = "Direct",
            FlightRegion = flightRegion
        };
    }

    private static DateTime? ParseDt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d) ? d : null;
    }

    private static string DetermineFlightRegion(string depIata, string arrIata)
    {
        var depTr = TurkishAirports.Contains(depIata);
        var arrTr = TurkishAirports.Contains(arrIata);
        return (depTr && arrTr) ? "Domestic" : "International";
    }

    /// <summary>
    /// Gercekci ucus fiyati hesaplar (TRY cinsinden)
    /// </summary>
    private static decimal CalculateRealisticPrice(string depIata, string arrIata, DateTime departureDate, string flightRegion)
    {
        // Temel fiyat
        decimal basePrice = flightRegion == "Domestic" ? 1500 : 5000;

        // Mesafe faktoru (basit yaklasim - gercek mesafe hesabi icin koordinat gerekir)
        var distanceFactor = CalculateDistanceFactor(depIata, arrIata);
        basePrice *= distanceFactor;

        // Tarih faktoru (yakin tarihler daha pahali)
        var daysUntilFlight = (departureDate.Date - DateTime.UtcNow.Date).Days;
        decimal dateFactor = 1.0m;
        if (daysUntilFlight < 3)
            dateFactor = 1.8m; // Son dakika %80 daha pahali
        else if (daysUntilFlight < 7)
            dateFactor = 1.5m; // 1 hafta oncesi %50 daha pahali
        else if (daysUntilFlight < 14)
            dateFactor = 1.3m; // 2 hafta oncesi %30 daha pahali
        else if (daysUntilFlight < 30)
            dateFactor = 1.1m; // 1 ay oncesi %10 daha pahali
        
        basePrice *= dateFactor;

        // Hafta sonu faktoru (Cuma-Pazar daha pahali)
        var dayOfWeek = departureDate.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            basePrice *= 1.2m;

        // Saat faktoru (sabah ve aksam saatleri daha pahali)
        var hour = departureDate.Hour;
        if ((hour >= 6 && hour <= 9) || (hour >= 17 && hour <= 20))
            basePrice *= 1.15m;

        // Rastgele varyasyon (%10 - %20 arasi)
        var random = new Random(depIata.GetHashCode() + arrIata.GetHashCode() + departureDate.GetHashCode());
        var variation = 0.9m + ((decimal)random.NextDouble() * 0.2m); // 0.9 - 1.1 arasi
        basePrice *= variation;

        // Yuvarla (50 TRY'nin katlarina)
        basePrice = Math.Round(basePrice / 50) * 50;

        // Minimum fiyat kontrolu
        var minPrice = flightRegion == "Domestic" ? 500 : 2000;
        return Math.Max(basePrice, minPrice);
    }

    /// <summary>
    /// Havalimanlari arasi mesafe faktoru (basitlestirilmis)
    /// </summary>
    private static decimal CalculateDistanceFactor(string depIata, string arrIata)
    {
        // Turkiye ic hatlar
        if (TurkishAirports.Contains(depIata) && TurkishAirports.Contains(arrIata))
        {
            // Kisa mesafe (Istanbul-Ankara gibi)
            if (IsShortDomesticRoute(depIata, arrIata))
                return 0.8m;
            // Orta mesafe (Istanbul-Izmir gibi)
            else if (IsMediumDomesticRoute(depIata, arrIata))
                return 1.0m;
            // Uzun mesafe (Istanbul-Trabzon gibi)
            else
                return 1.3m;
        }

        // Avrupa
        if (IsEuropeanRoute(depIata, arrIata))
            return 1.5m;

        // Orta Dogu
        if (IsMiddleEastRoute(depIata, arrIata))
            return 1.8m;

        // Uzak Dogu / Amerika
        return 3.0m;
    }

    private static bool IsShortDomesticRoute(string dep, string arr)
    {
        var shortRoutes = new[] { "IST-ESB", "IST-ADB", "SAW-ESB", "SAW-ADB" };
        var route = $"{dep}-{arr}";
        var reverseRoute = $"{arr}-{dep}";
        return shortRoutes.Contains(route) || shortRoutes.Contains(reverseRoute);
    }

    private static bool IsMediumDomesticRoute(string dep, string arr)
    {
        var mediumRoutes = new[] { "IST-AYT", "ESB-AYT", "ADB-AYT", "IST-TZX" };
        var route = $"{dep}-{arr}";
        var reverseRoute = $"{arr}-{dep}";
        return mediumRoutes.Contains(route) || mediumRoutes.Contains(reverseRoute);
    }

    private static bool IsEuropeanRoute(string dep, string arr)
    {
        var europeanAirports = new[] { "LHR", "LGW", "CDG", "ORY", "FRA", "MUC", "AMS", "FCO", "MAD", "BCN", "VIE", "ZRH" };
        return europeanAirports.Contains(dep) || europeanAirports.Contains(arr);
    }

    private static bool IsMiddleEastRoute(string dep, string arr)
    {
        var middleEastAirports = new[] { "DXB", "DWC", "DOH", "CAI", "JED", "RUH" };
        return middleEastAirports.Contains(dep) || middleEastAirports.Contains(arr);
    }
}
