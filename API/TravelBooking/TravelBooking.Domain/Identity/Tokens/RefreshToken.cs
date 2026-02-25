using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Identity.Tokens;

public sealed class RefreshToken : BaseEntity
{
    public string AppUserId { get; private set; } = string.Empty;

    // Store only hashed token
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string appUserId, string tokenHash, DateTime expiresAtUtc)
    {
        AppUserId = appUserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc is not null;

    public void Revoke() => RevokedAtUtc = DateTime.UtcNow;
}
