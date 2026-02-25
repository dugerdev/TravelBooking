using TravelBooking.Web.DTOs.Enums;

namespace TravelBooking.Web.DTOs.Passengers;

public class CreatePassengerDto
{
    public string PassengerFirstName { get; set; } = string.Empty;
    public string PassengerLastName { get; set; } = string.Empty;
    public string? NationalNumber { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PassengerType { get; set; } = "Adult";
}
