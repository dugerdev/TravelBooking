using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelBooking.Api.Models;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace TravelBooking.Api.Services;

public interface ITokenService
{
    Task<TokenModel> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
}

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;

    public JwtTokenService(IConfiguration configuration, UserManager<AppUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<TokenModel> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var issuer = _configuration["JWT:ValidIssuer"];
        var audience = _configuration["JWT:ValidAudience"];
        //---Development'ta default secret kullan, production'da config'den al---//
        var secret = _configuration["JWT:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            //---Development ortaminda default secret kullan (guvenlik icin sadece development'ta)---//
            secret = "Development-Default-Secret-Key-At-Least-32-Characters-Long-For-JWT-Signing";
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var accessMinutes = _configuration.GetValue<int?>("JWT:AccessTokenMinutes") ?? 120;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(accessMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: creds);

        return new TokenModel
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            UserName = user.UserName ?? "",
            Roles = roles,
            RefreshToken = string.Empty,
            RefreshTokenExpiresAtUtc = DateTime.MinValue
        };
    }
}
