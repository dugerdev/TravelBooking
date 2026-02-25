using Microsoft.AspNetCore.Authorization;

namespace TravelBooking.Api.Authorization;

/// <summary>
/// Yetkilendirme policy'lerinin tanimlandigi sinif
/// Policy'ler, kullanicilarin hangi endpoint'lere erisebilecegini kontrol eder
/// </summary>
public static class Policies
{
    // Sadece Admin rolune sahip kullanicilar erisebilir
    public const string AdminOnly = "AdminOnly";

    // User veya Admin rolune sahip kullanicilar erisebilir
    public const string UserOrAdmin = "UserOrAdmin";

    // Kullanici kendi kaynagina erisebilir veya Admin rolune sahip olabilir
    // Ornek: Kullanici sadece kendi rezervasyonlarini gorebilir, Admin tum rezervasyonlari gorebilir
    public const string ResourceOwnerOrAdmin = "ResourceOwnerOrAdmin";

    /// <summary>
    /// Yetkilendirme policy'lerini yapilandirir
    /// Bu metod Program.cs'de cagrilir ve tum policy'leri uygular
    /// </summary>
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // AdminOnly: Sadece Admin rolune sahip kullanicilar erisebilir
        options.AddPolicy(AdminOnly, policy => policy.RequireRole("Admin"));

        // UserOrAdmin: User veya Admin rolune sahip kullanicilar erisebilir
        options.AddPolicy(UserOrAdmin, policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("User") || context.User.IsInRole("Admin")));

        // ResourceOwnerOrAdmin: Kullanici kendi kaynagina erisebilir veya Admin olabilir
        // Bu policy, kullanicinin kendi ID'si ile route/query'deki ID'yi karsilastirir
        options.AddPolicy(ResourceOwnerOrAdmin, policy =>
            policy.RequireAssertion(context =>
            {
                // Admin ise her zaman erisebilir
                if (context.User.IsInRole("Admin"))
                    return true;

                // AuthorizationHandlerContext'ten HttpContext'e eris
                // HttpContext'e erismek icin Resource'u HttpContext olarak cast etmeliyiz
                var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
                if (httpContext == null)
                    return false;

                // Route'dan userId veya id parametresini al (orn: /api/users/{userId} veya /api/reservations/{id})
                var routeData = httpContext.Request.RouteValues;
                var userIdFromRoute = routeData["userId"]?.ToString() ??
                                     routeData["id"]?.ToString();

                // Query string'den userId al (orn: ?userId=123)
                var userIdFromQuery = httpContext.Request.Query["userId"].ToString();

                // JWT token'dan mevcut kullanici ID'sini al (NameIdentifier veya sub claim'i)
                var currentUserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                                   context.User.FindFirst("sub")?.Value;

                // Hedef kullanici ID'sini belirle (route veya query'den)
                var targetUserId = userIdFromRoute ?? userIdFromQuery;

                // Mevcut kullanici ID'si ve hedef kullanici ID'si varsa ve esitse erisim ver
                // Bu sayede kullanici sadece kendi kaynaklarina erisebilir
                return !string.IsNullOrEmpty(currentUserId) &&
                       !string.IsNullOrEmpty(targetUserId) &&
                       currentUserId == targetUserId;
            }));
    }
}
