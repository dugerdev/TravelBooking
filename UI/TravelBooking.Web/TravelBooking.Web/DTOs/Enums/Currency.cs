using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    TRY = 1,
    USD = 2,
    EUR = 3,
    GBP = 4,
    JPY = 5
}
