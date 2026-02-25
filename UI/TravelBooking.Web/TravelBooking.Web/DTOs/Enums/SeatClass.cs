using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatClass
{
    Economy = 1,
    PremiumEconomy = 2,
    Business = 3,
    First = 4,
    FirstClass = 4 // Alias for First
}
