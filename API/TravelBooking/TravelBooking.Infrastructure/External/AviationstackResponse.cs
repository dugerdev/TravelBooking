using TravelBooking.Application.Dtos.External;
using System.Text.Json.Serialization;

namespace TravelBooking.Infrastructure.External;

//---Aviationstack API response yapisi---//
internal class AviationstackApiResponse
{
    [JsonPropertyName("data")]
    public List<AviationstackFlightData>? Data { get; set; }

    [JsonPropertyName("error")]
    public AviationstackError? Error { get; set; }
}

internal class AviationstackFlightData
{
    [JsonPropertyName("flight")]
    public AviationstackFlight? Flight { get; set; }

    [JsonPropertyName("airline")]
    public AviationstackAirline? Airline { get; set; }

    [JsonPropertyName("departure")]
    public AviationstackAirportInfo? Departure { get; set; }

    [JsonPropertyName("arrival")]
    public AviationstackAirportInfo? Arrival { get; set; }

    [JsonPropertyName("aircraft")]
    public AviationstackAircraft? Aircraft { get; set; }
}

internal class AviationstackFlight
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }
}

internal class AviationstackAirline
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }
}

internal class AviationstackAirportInfo
{
    [JsonPropertyName("airport")]
    public string? Airport { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }

    [JsonPropertyName("scheduled")]
    public DateTime? Scheduled { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}

internal class AviationstackAircraft
{
    [JsonPropertyName("registration")]
    public string? Registration { get; set; }

    [JsonPropertyName("iata")]
    public string? Iata { get; set; }

    [JsonPropertyName("icao")]
    public string? Icao { get; set; }
}

internal class AviationstackError
{
    //---Aviationstack API'den gelen error.code hem string hem int olabilir---//
    //---object olarak tanimlayip ToString() ile string'e ceviriyoruz---//
    [JsonPropertyName("code")]
    public object? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("context")]
    public Dictionary<string, object>? Context { get; set; }

    //---Code'u string olarak donduren helper property---//
    public string? CodeAsString => Code?.ToString();
}
