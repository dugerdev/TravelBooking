using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Dtos;

public sealed class PassengerDto
{
    public Guid Id { get; set; }
    public string PassengerFirstName { get; set; } = string.Empty;
    public string PassengerLastName { get; set; } = string.Empty;
    public string? NationalNumber { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public PassengerType PassengerType { get; set; }
    public DateTime CreatedDate { get; set; }
}
