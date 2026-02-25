using System.Text.Json.Serialization;

namespace TravelBooking.Infrastructure.External;

/// <summary>AeroDataBox FIDS endpoint response: /flights/airports/iata/{code}/{fromLocal}/{toLocal}.</summary>
internal class AeroDataBoxFidsResponse
{
    [JsonPropertyName("departures")]
    public List<AeroDataBoxFlight>? Departures { get; set; }

    [JsonPropertyName("arrivals")]
    public List<AeroDataBoxFlight>? Arrivals { get; set; }
}

internal class AeroDataBoxFlight
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("departure")]
    public AeroDataBoxMovement? Departure { get; set; }

    [JsonPropertyName("arrival")]
    public AeroDataBoxMovement? Arrival { get; set; }

    [JsonPropertyName("airline")]
    public AeroDataBoxAirline? Airline { get; set; }
}

internal class AeroDataBoxMovement
{
    [JsonPropertyName("airport")]
    public AeroDataBoxAirport? Airport { get; set; }

    [JsonPropertyName("scheduledTime")]
    public AeroDataBoxDateTime? ScheduledTime { get; set; }

    [JsonPropertyName("revisedTime")]
    public AeroDataBoxDateTime? RevisedTime { get; set; }
}

internal class AeroDataBoxAirport
{
    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("municipalityName")]
    public string? MunicipalityName { get; set; }
}

internal class AeroDataBoxDateTime
{
    [JsonPropertyName("utc")]
    public string? Utc { get; set; }

    [JsonPropertyName("local")]
    public string? Local { get; set; }
}

internal class AeroDataBoxAirline
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }
}
