namespace TravelBooking.Application.Dtos;

public class UpdateUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? LockoutEnabled { get; set; }
}
