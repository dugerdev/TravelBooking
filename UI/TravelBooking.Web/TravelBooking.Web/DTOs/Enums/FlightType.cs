using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlightType
{
    Direct = 1,
    Connecting = 2,
    Charter = 3
}
