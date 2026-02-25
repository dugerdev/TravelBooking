namespace TravelBooking.Web.DTOs.Passengers;

public class PassengerDto
{
    public Guid Id { get; set; }
    public string PassengerFirstName { get; set; } = string.Empty;
    public string PassengerLastName { get; set; } = string.Empty;
    public string? NationalNumber { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PassengerType { get; set; } = "Adult";
    public string? Email { get; set; }

    /// <summary>Alias for PassengerFirstName for view compatibility.</summary>
    public string FirstName { get => PassengerFirstName; set => PassengerFirstName = value ?? string.Empty; }

    /// <summary>Alias for PassengerLastName for view compatibility.</summary>
    public string LastName { get => PassengerLastName; set => PassengerLastName = value ?? string.Empty; }

    /// <summary>Nullable date of birth for view display (DateTime? from DateOfBirth).</summary>
    public DateTime? DateOfBirthDisplay => DateOfBirth == default ? null : DateOfBirth;
}
