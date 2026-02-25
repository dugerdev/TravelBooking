using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationType
{
    Flight = 1,
    Hotel = 2,
    Car = 3,
    Tour = 4
}
