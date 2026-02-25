using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BaggageOption
{
    Light = 1,
    Standard = 2,
    Plus = 3,
    Heavy = 4,
    Business = 5
}
