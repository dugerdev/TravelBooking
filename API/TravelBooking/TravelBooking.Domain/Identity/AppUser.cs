using System;
using Microsoft.AspNetCore.Identity;

namespace TravelBooking.Domain.Identity;

// Identity user is infrastructure-ish, but you requested it in Domain.
// Use this to extend user profile later (Name, Surname, etc.)
public class AppUser : IdentityUser
{
    // Audit fields (table has non-null CreatedDate)
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TravelBooking.Domain.Entities.Reservation> Reservations { get; set; } = [];

    public ICollection<TravelBooking.Domain.Identity.Tokens.RefreshToken> RefreshTokens { get; set; } = [];
}
