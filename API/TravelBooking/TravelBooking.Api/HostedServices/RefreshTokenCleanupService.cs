using TravelBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Api.HostedServices;

public sealed class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //---Migration'lar tamamlanana kadar bekle---//
        await WaitForMigrationsAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TravelBookingDbContext>();

                //---RefreshTokens tablosunun var olup olmadigini kontrol et---//
                if (!await TableExistsAsync(db, "RefreshTokens"))
                {
                    _logger.LogWarning("RefreshTokens tablosu bulunamadi. Migration'lar tamamlanmamis olabilir. 5 dakika sonra tekrar denenecek.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                var now = DateTime.UtcNow;

                // delete expired tokens (and revoked tokens older than 30 days)
                var retentionDays = _configuration.GetValue<int?>("RefreshTokenCleanup:RevokedRetentionDays") ?? 30;
                var cutoffRevoked = now.AddDays(-retentionDays);

                var expired = await db.RefreshTokens
                    .Where(t => t.ExpiresAtUtc <= now || (t.RevokedAtUtc != null && t.RevokedAtUtc <= cutoffRevoked))
                    .ToListAsync(stoppingToken);

                if (expired.Count > 0)
                {
                    db.RefreshTokens.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Refresh token cleanup deleted {Count} tokens.", expired.Count);
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
            {
                _logger.LogWarning(sqlEx, "RefreshTokens tablosu bulunamadi. Migration'lar tamamlanmamis olabilir. 5 dakika sonra tekrar denenecek.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token cleanup failed.");
            }

            var intervalHours = _configuration.GetValue<int?>("RefreshTokenCleanup:IntervalHours") ?? 6;
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }

    //---Migration'lar tamamlanana kadar bekleyen metot---//
    private async Task WaitForMigrationsAsync(CancellationToken stoppingToken)
    {
        const int maxWaitMinutes = 10;
        const int checkIntervalSeconds = 5;
        var startTime = DateTime.UtcNow;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TravelBookingDbContext>();
                
                //---RefreshTokens tablosunun var olup olmadigini kontrol et---//
                if (await TableExistsAsync(db, "RefreshTokens"))
                {
                    _logger.LogInformation("RefreshTokens tablosu bulundu. Migration'lar tamamlanmis gorunuyor.");
                    return;
                }
                
                //---Maksimum bekleme suresini kontrol et---//
                if (DateTime.UtcNow - startTime > TimeSpan.FromMinutes(maxWaitMinutes))
                {
                    _logger.LogWarning("Migration'lar {Minutes} dakika icinde tamamlanmadi. Background service devam edecek ama tablo kontrolu yapilacak.", maxWaitMinutes);
                    return;
                }
                
                _logger.LogInformation("Migration'lar tamamlanmayi bekliyor... RefreshTokens tablosu henuz yok. {ElapsedSeconds} saniye gecti.", (DateTime.UtcNow - startTime).TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Migration kontrolu sirasinda hata olustu. 5 saniye sonra tekrar denenecek.");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(checkIntervalSeconds), stoppingToken);
        }
    }

    //---Tablonun var olup olmadigini kontrol eden metot---//
    private static async Task<bool> TableExistsAsync(TravelBookingDbContext db, string tableName)
    {
        try
        {
            //---INFORMATION_SCHEMA kullanarak tablo varligini kontrol et (exception firlatmaz)---//
            //---SqlQueryRaw ile scalar deger donduren sorgu yap---//
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}) THEN 1 ELSE 0 END AS [Value]",
                tableName);
            
            var exists = await existsQuery.SingleAsync();
            return exists == 1;
        }
        catch
        {
            //---Hata durumunda false dondur---//
            return false;
        }
    }
}
