using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class CreatePassengerDto
{
    public string PassengerFirstName { get; set; } = string.Empty;
    public string PassengerLastName { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public PassengerType PassengerType { get; set; } = PassengerType.Adult;
}

