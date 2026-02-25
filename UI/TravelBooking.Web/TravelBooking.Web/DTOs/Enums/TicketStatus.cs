using System.Text.Json.Serialization;

namespace TravelBooking.Web.DTOs.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    Reserved = 1,
    Confirmed = 2,
    CheckedIn = 3,
    Cancelled = 4
}
