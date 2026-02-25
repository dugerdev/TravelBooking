using System.Security.Cryptography;
using System.Text;
using TravelBooking.Domain.Identity.Tokens;
using TravelBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Api.Services.Auth;

public interface IRefreshTokenService
{
    Task<(string refreshToken, DateTime expiresAtUtc)> IssueAsync(string userId, CancellationToken cancellationToken);
    Task<(string userId, string newRefreshToken, DateTime newExpiresAtUtc)> RotateAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly TravelBookingDbContext _db;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(TravelBookingDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    //Örnek: Yeni refresh token uretir; sifreyi DB'de tutmamak icin SHA256 ile hash'leyip saklar
    public async Task<(string refreshToken, DateTime expiresAtUtc)> IssueAsync(string userId, CancellationToken cancellationToken)
    {
        var refreshToken = GenerateToken();
        var hash = Sha256(refreshToken);
        var refreshDays = _configuration.GetValue<int?>("JWT:RefreshTokenDays") ?? 14;
        var expiresAtUtc = DateTime.UtcNow.AddDays(refreshDays);

        var entity = new RefreshToken(userId, hash, expiresAtUtc);
        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return (refreshToken, expiresAtUtc);
    }

    //Örnek: Token rotation - eski token gecersiz kilinir, yeni token uretilir; reuse detection ile calinmis token kullanilirsa tum tokenlar iptal edilir
    public async Task<(string userId, string newRefreshToken, DateTime newExpiresAtUtc)> RotateAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = Sha256(refreshToken);
        var tokenEntity = await _db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (tokenEntity is null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        //Örnek: Reuse detection - iptal edilmis token tekrar kullanilirsa (token calinti olabilir) kullanicinin tum aktif tokenlari iptal edilir
        if (tokenEntity.IsRevoked)
        {
            var activeTokens = await _db.RefreshTokens
                .AsTracking()
                .Where(x => x.AppUserId == tokenEntity.AppUserId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var t in activeTokens)
                t.Revoke();

            await _db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Refresh token reuse detected");
        }

        if (tokenEntity.IsExpired)
            throw new UnauthorizedAccessException("Invalid refresh token");

        //Örnek: Eski token iptal edilir, yeni token uretilir (her kullanimda tek kullanimlik)
        tokenEntity.Revoke();

        //Örnek: Yeni refresh token verilir
        var (newToken, newExpiresAtUtc) = await IssueAsync(tokenEntity.AppUserId, cancellationToken);

        return (tokenEntity.AppUserId, newToken, newExpiresAtUtc);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = Sha256(refreshToken);
        var tokenEntity = await _db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (tokenEntity is null) return;

        if (!tokenEntity.IsRevoked)
        {
            tokenEntity.Revoke();
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    //Örnek: Kriptografik olarak guvenli rastgele 64 byte token uretir
    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    //Örnek: Token'i DB'de saklamak icin SHA256 hash; sifre kirmaya karsi koruma
    private static string Sha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash); // 64 chars
    }
}
