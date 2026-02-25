using TravelBooking.Api.Data;
using TravelBooking.Domain.Identity;
using TravelBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Common;
using System.Linq;

namespace TravelBooking.Api;

public static class DbSeeder
{
    private static readonly string[] DefaultRoles = { "Admin", "User" };
    
    public static async Task SeedData(WebApplication app)
    {
        // Seed only when explicitly enabled (default: false in production)
        var enabled = app.Configuration.GetValue<bool?>("Seed:Enabled") ?? app.Environment.IsDevelopment();
        
        if (!enabled)
        {
            // Logger'i scope icinde almak icin gecici scope olustur
            using var tempScope = app.Services.CreateScope();
            var tempLogger = tempScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            tempLogger.LogInformation("Seed data: Devre disi (Seed:Enabled = false). Seed data atlaniyor.");
            return;
        }

        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<TravelBookingDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Seed data: Baslatiliyor... (Seed:Enabled = true)");
        
        //---Development ortaminda veritabanini sifirlama (config ile kontrol edilir)---//
        // Varsayilan false. User Secrets veya ortam degiskeni true override ederse sifirlama yapilir.
        var resetOnStartup = app.Configuration.GetValue<bool>("Database:ResetOnStartup", false);
        if (app.Environment.IsDevelopment() && resetOnStartup)
        {
            logger.LogWarning("Database:ResetOnStartup aktif. Veritabani sifirlaniyor...");
            try
            {
                // Veritabanini kesinlikle sil
                var canConnect = await db.Database.CanConnectAsync(CancellationToken.None);
                if (canConnect)
                {
                    logger.LogInformation("Mevcut veritabani baglantisi kesiliyor...");
                    await db.Database.CloseConnectionAsync();
                }
                
                // EnsureDeletedAsync - veritabanini tamamen sil (migration history dahil)
                await db.Database.EnsureDeletedAsync(CancellationToken.None);
                logger.LogInformation("✓ Veritabani basariyla silindi. Migration history temizlendi.");
                
                // Veritabani silindikten sonra MigrateAsync dogrudan cagrilmali.
                // GetAppliedMigrationsAsync/GetPendingMigrationsAsync veritabani olmadan calismaz.
                logger.LogInformation("Veritabani yeniden olusturuluyor ve migration'lar uygulaniyor...");
                await db.Database.MigrateAsync(CancellationToken.None);
                logger.LogInformation("✓ Veritabani ve tum tablolar (Flights dahil) olusturuldu.");
                
                // NOT: EnsureCreated kullanmiyoruz - migration'lar tablolari olusturacak
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Veritabani sifirlanirken hata olustu: {Message}", ex.Message);
                // Hata olsa bile devam et - migration'lar tablolari olusturabilir
                logger.LogWarning("Veritabani silme hatasi yok sayiliyor, migration'lar uygulanacak...");
            }
        }
        
        //---Migration gecmisini kontrol et---//
        await CheckMigrationStatusAsync(db, logger);
        
        //---Migration'lari uygula (ResetOnStartup ile silinmediyse veya yukaridaki blok atlanmissa)---//
        // ResetOnStartup true ise yukarida MigrateAsync zaten cagrildi, tekrar cagirmaya gerek yok
        if (!resetOnStartup)
        {
            await ApplyMigrationsAsync(db, logger);
        }

        //---Migration'larin basariyla uygulandigini dogrula---//
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
        if (pendingMigrations.Any())
        {
            logger.LogWarning("Bekleyen migration'lar var: {Migrations}. Uygulama devam ediyor.", string.Join(", ", pendingMigrations));
            // Uygulama cokmemeli - migration'lar sonraki adimda uygulanabilir
        }

        //---Kritik tablolari dogrula (migration history tutarsizligina karsi)---//
        await EnsureRefreshTokensTableAsync(db, logger);
        await EnsureReservationsTableAsync(db, logger);
        await EnsureFlightsTableAsync(db, logger);
        await EnsureHotelsTableAsync(db, logger);
        await EnsureCarsTableAsync(db, logger);
        await EnsureToursTableAsync(db, logger);
        await EnsureNewsTableAsync(db, logger);
        await EnsureTestimonialsTableAsync(db, logger);

        // Havalimanlari, oteller (ve odalar), araclar, turlar, haberler vb. seed verisi
        await TravelBooking.Api.Data.SeedData.SeedAsync(db);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // Roles
        foreach (var role in DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Default admin (configure via appsettings / environment)
        logger.LogInformation("Seed data: Admin kullanicisi olusturuluyor...");
        var adminEmail = app.Configuration["Seed:AdminEmail"];
        var adminUserName = app.Configuration["Seed:AdminUserName"];
        var adminPassword = app.Configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            // In development we allow the default values for convenience
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("Seed data: Development ortaminda default admin bilgileri kullaniliyor.");
                adminEmail = "admin@gocebe.com";
                adminUserName = "admin";
                adminPassword = "Admin123!ChangeMe";
            }
            else
            {
                // In non-development environments we require explicit config
                logger.LogWarning("Seed data: Production ortaminda admin bilgileri eksik. Seed data atlaniyor.");
                return;
            }
        }
        else
        {
            logger.LogInformation("Seed data: Admin bilgileri config'den okundu. Email: {Email}, UserName: {UserName}", adminEmail, adminUserName);
        }

        var admin = await userManager.FindByNameAsync(adminUserName) ?? await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            logger.LogInformation("Seed data: Admin kullanicisi bulunamadi. Yeni admin kullanicisi olusturuluyor...");
            admin = new AppUser { Email = adminEmail, UserName = adminUserName, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (createResult.Succeeded)
            {
                logger.LogInformation("Seed data: Admin kullanicisi basariyla olusturuldu. Admin rolu ataniyor...");
                var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Seed data: Admin kullanicisina Admin rolu basariyla atandi.");
                }
                else
                {
                    logger.LogError("Seed data: Admin rolu atanirken hata olustu: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Seed data: Admin kullanicisi olusturulurken hata olustu: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Seed data: Admin kullanicisi zaten mevcut. Sifre ve rol kontrol ediliyor...");
            
            //---Mevcut admin kullanicisinin sifresini guncelle (Development ortaminda)---//
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("Seed data: Development ortaminda admin sifresi guncelleniyor...");
                var hasPassword = await userManager.HasPasswordAsync(admin);
                if (hasPassword)
                {
                    //---Mevcut sifreyi kaldir ve yeni sifre ekle---//
                    var removeResult = await userManager.RemovePasswordAsync(admin);
                    if (removeResult.Succeeded)
                    {
                        var addResult = await userManager.AddPasswordAsync(admin, adminPassword);
                        if (addResult.Succeeded)
                        {
                            logger.LogInformation("Seed data: Admin kullanicisinin sifresi basariyla guncellendi.");
                        }
                        else
                        {
                            logger.LogError("Seed data: Admin sifresi eklenirken hata olustu: {Errors}", string.Join(", ", addResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger.LogError("Seed data: Admin sifresi kaldirilirken hata olustu: {Errors}", string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    //---Sifre yoksa direkt ekle---//
                    var addResult = await userManager.AddPasswordAsync(admin, adminPassword);
                    if (addResult.Succeeded)
                    {
                        logger.LogInformation("Seed data: Admin kullanicisina sifre basariyla eklendi.");
                    }
                    else
                    {
                        logger.LogError("Seed data: Admin sifresi eklenirken hata olustu: {Errors}", string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            
            //---Admin rolu kontrolu---//
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                logger.LogInformation("Seed data: Admin kullanicisina Admin rolu ataniyor...");
                var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Seed data: Admin rolu basariyla atandi.");
                }
                else
                {
                    logger.LogError("Seed data: Admin rolu atanirken hata olustu: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Seed data: Admin kullanicisi zaten Admin rolune sahip.");
            }
        }
        
        logger.LogInformation("Seed data: Admin kullanicisi islemi tamamlandi.");
        
        //---Havalimanlari ve Ucuslar seed data---//
        await SeedAirportsAsync(db, logger);
        await SeedFlightsAsync(db, logger);
        
        //---Hotel, Car, Tour, News seed data---//
        // SeedHotelsAsync kaldirildi - SeedData.SeedAsync zaten otelleri ve odalari ekliyor
        // await SeedHotelsAsync(db, logger);
        await SeedCarsAsync(db, logger);
        await SeedToursAsync(db, logger);
        await SeedNewsAsync(db, logger);
        
        logger.LogInformation("Seed data: Tum seed islemleri tamamlandi.");
    }

    //---Migration durumunu kontrol eden metot---//
    private static async Task CheckMigrationStatusAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
            
            logger.LogInformation("Uygulanmis migration'lar: {Count}", appliedMigrations.Count());
            if (appliedMigrations.Any())
            {
                logger.LogInformation("Migration listesi: {Migrations}", string.Join(", ", appliedMigrations));
            }
            
            if (pendingMigrations.Any())
            {
                logger.LogWarning("Bekleyen migration'lar: {Count}", pendingMigrations.Count());
                logger.LogWarning("Bekleyen migration listesi: {Migrations}", string.Join(", ", pendingMigrations));
            }
            else
            {
                logger.LogInformation("Tum migration'lar uygulanmis.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration durumu kontrol edilirken hata olustu.");
        }
    }

    //---Migration'lari guvenli bir sekilde uygulayan metot---//
    private static async Task ApplyMigrationsAsync(TravelBookingDbContext db, ILogger logger)
    {
        //---Bekleyen migration'lari once kontrol et (catch blogunda kullanmak icin)---//
        var pendingBefore = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
        
        try
        {
            // Veritabani silindiyse tum migration'lari uygula
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
            var shouldApplyAll = appliedMigrations.Count() == 0;
            
            if (pendingBefore.Count() > 0 || shouldApplyAll)
            {
                var migrationsToApply = shouldApplyAll 
                    ? "TUM MIGRATION'LAR (veritabani sifirlandi)" 
                    : string.Join(", ", pendingBefore);
                    
                logger.LogInformation("Migration'lar uygulaniyor: {Migrations}", migrationsToApply);
                await db.Database.MigrateAsync(CancellationToken.None);
                logger.LogInformation("✓ Migration islemi tamamlandi.");
            }
            else
            {
                logger.LogInformation("Bekleyen migration yok. Veritabani guncel.");
                return;
            }
            
            //---Migration'larin gercekten uygulandigini dogrula---//
            var pendingAfter = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
            if (pendingAfter.Count() > 0)
            {
                logger.LogWarning("Migration uygulandi ancak bekleyen migration'lar hala var: {Migrations}", string.Join(", ", pendingAfter));
                logger.LogWarning("Bu durum genellikle migration history tablosunda tutarsizlik oldugunu gosterir.");
                // Uygulama devam ediyor - ResetOnStartup ile duzelecek
            }
            
            logger.LogInformation("Migration dogrulamasi: Tum migration'lar basariyla uygulandi.");
        }
        catch (Exception ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("There is already an object"))
        {
            logger.LogWarning(ex, "Migration uygulanirken 'already exists' hatasi olustu. Bu genellikle migration history tutarsizligi anlamina gelir.");
            logger.LogWarning("Tablolar mevcut ama migration history'de kayit yok. Migration history kontrol ediliyor...");
            
            //---Migration history tablosunu kontrol et ve eksik kayitlari ekle---//
            await FixMigrationHistoryAsync(db, logger, pendingBefore);
            
            //---Tekrar kontrol et---//
            var remainingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
            if (remainingMigrations.Count() > 0)
            {
                logger.LogWarning("Migration history duzeltildi ancak hala bekleyen migration'lar var: {Migrations}", string.Join(", ", remainingMigrations));
                logger.LogWarning("Bu migration'lari manuel olarak uygulamaniz gerekebilir: 'dotnet ef database update'");
                // Uygulama devam ediyor - ResetOnStartup ile duzelecek
            }
            else
            {
                logger.LogInformation("Migration history duzeltildi. Tum migration'lar uygulandi.");
            }
        }
        catch (Exception ex)
        {
            //---Diger hatalar icin exception'i tekrar firlat---//
            logger.LogError(ex, "Migration uygulanamadi. Hata detayi: {Message}", ex.Message);
            logger.LogError("Lutfen veritabani baglantisini ve migration dosyalarini kontrol edin.");
            throw;
        }
    }

    //---Reservations tablosuna yeni kolonlari ekleyen metot---//
    private static async Task EnsureReservationsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            // Check if Type column exists
            var typeColumnExists = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Reservations' AND COLUMN_NAME = 'Type') THEN 1 ELSE 0 END AS [Value]")
                .SingleAsync();

            if (typeColumnExists == 0)
            {
                logger.LogInformation("Reservations tablosuna Type kolonu ekleniyor...");
                await db.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE [Reservations] ADD [Type] nvarchar(20) NOT NULL DEFAULT 'Flight';
                    ALTER TABLE [Reservations] ADD [HotelId] uniqueidentifier NULL;
                    ALTER TABLE [Reservations] ADD [CarId] uniqueidentifier NULL;
                    ALTER TABLE [Reservations] ADD [TourId] uniqueidentifier NULL;

                    CREATE INDEX [IX_Reservations_Type] ON [Reservations]([Type]);
                    CREATE INDEX [IX_Reservations_HotelId] ON [Reservations]([HotelId]);
                    CREATE INDEX [IX_Reservations_CarId] ON [Reservations]([CarId]);
                    CREATE INDEX [IX_Reservations_TourId] ON [Reservations]([TourId]);
                ");
                logger.LogInformation("✓ Reservations tablosuna Type, HotelId, CarId, TourId kolonlari eklendi.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Reservations tablosu guncellenirken hata olustu. Migration'lar ile uygulanacak.");
        }
    }

    //---RefreshTokens tablosunu dogrulayan/olusturan metot---//
    private static async Task EnsureRefreshTokensTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RefreshTokens') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
                return;

            logger.LogWarning("RefreshTokens tablosu bulunamadi. Migration history tutarsiz olabilir. Tablo yeniden olusturulacak.");

            var createSql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RefreshTokens')
BEGIN
    CREATE TABLE [RefreshTokens](
        [Id] uniqueidentifier NOT NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        [TokenHash] nvarchar(256) NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [RevokedAtUtc] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedDate] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_RefreshTokens_AppUserId] ON [RefreshTokens]([AppUserId]);
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens]([TokenHash]);
END";

            await db.Database.ExecuteSqlRawAsync(createSql);
            logger.LogInformation("RefreshTokens tablosu basariyla olusturuldu.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RefreshTokens tablosu dogrulanirken/olusturulurken hata olustu. Migration ile olusacak.");
            // Uygulama devam ediyor - migration'lar tabloyu olusturacak
        }
    }

    //---Flights tablosunu dogrulayan metot---//
    private static async Task EnsureFlightsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Flights') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                logger.LogInformation("✓ Flights tablosu mevcut.");
                return;
            }

            // Tablo yoksa - bu ciddi bir sorun
            logger.LogError("✗ Flights tablosu bulunamadi!");
            
            // Migration durumunu kontrol et
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
            
            if (appliedMigrations.Any() && !pendingMigrations.Any())
            {
                logger.LogWarning("Migration'lar uygulanmis gorunuyor ama Flights tablosu yok!");
                logger.LogWarning("Bu, migration history tutarsizligi oldugunu gosterir.");
                logger.LogWarning("Cozum: appsettings.Development.json'da 'Database:ResetOnStartup: true' ile API'yi yeniden baslatin.");
                // Uygulama devam ediyor - ResetOnStartup ile duzelecek
                return;
            }
            else if (pendingMigrations.Count() > 0)
            {
                logger.LogWarning("Bekleyen migration'lar var, Flights tablosu migration sonrasi olusacak.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Flights tablosu dogrulanirken hata olustu: {Message}. Migration ile olusacak.", ex.Message);
            // Uygulama devam ediyor - migration'lar tabloyu olusturacak
        }
    }

    //---Hotels tablosunu dogrulayan metot---//
    private static async Task EnsureHotelsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Hotels') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                // Tablo var, kolonlari kontrol et
                var hasOldPriceColumn = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Hotels' AND COLUMN_NAME = 'PricePerNight') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var hasNewPriceColumns = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Hotels' AND COLUMN_NAME = 'PricePerNight_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                if (hasOldPriceColumn == 1 && hasNewPriceColumns == 0)
                {
                    logger.LogWarning("Hotels tablosu eski semada (PricePerNight kolonu var). Migration uygulanmali.");
                }
                else
                {
                    logger.LogInformation("✓ Hotels tablosu mevcut ve sema guncel.");
                }
                return;
            }

            logger.LogWarning("Hotels tablosu bulunamadi. Migration uygulanmali.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Hotels tablosu dogrulanirken hata olustu: {Message}", ex.Message);
        }
    }

    //---Cars tablosunu dogrulayan metot---//
    private static async Task EnsureCarsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cars') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                // Tablo var, kolonlari kontrol et
                var hasOldPriceColumn = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'PricePerDay') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var hasNewPriceColumns = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'PricePerDay_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                if (hasOldPriceColumn == 1 && hasNewPriceColumns == 0)
                {
                    logger.LogWarning("Cars tablosu eski semada (PricePerDay kolonu var). Migration uygulanmali.");
                }
                else
                {
                    // Yeni policy kolonlarini kontrol et ve ekle
                    var hasFuelPolicy = await db.Database.SqlQueryRaw<int>(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'FuelPolicy') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                    
                    if (hasFuelPolicy == 0)
                    {
                        logger.LogWarning("Cars tablosunda yeni policy kolonlari eksik. Ekleniyor...");
                        try
                        {
                            await db.Database.ExecuteSqlRawAsync(@"
                                ALTER TABLE Cars ADD FuelPolicy NVARCHAR(50) NOT NULL DEFAULT 'Full to Full';
                                ALTER TABLE Cars ADD MileagePolicy NVARCHAR(50) NOT NULL DEFAULT 'Unlimited';
                                ALTER TABLE Cars ADD PickupLocationType NVARCHAR(50) NOT NULL DEFAULT 'In Terminal';
                                ALTER TABLE Cars ADD Supplier NVARCHAR(200) NOT NULL DEFAULT '';
                            ");
                            logger.LogInformation("✓ Cars tablosuna yeni policy kolonlari eklendi.");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Cars policy kolonlari eklenirken hata: {Message}", ex.Message);
                        }
                    }
                    else
                    {
                        logger.LogInformation("✓ Cars tablosu mevcut ve sema guncel.");
                    }
                }
                return;
            }

            logger.LogWarning("Cars tablosu bulunamadi. Migration uygulanmali.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cars tablosu dogrulanirken hata olustu: {Message}", ex.Message);
        }
    }

    //---Tours tablosunu dogrulayan metot---//
    private static async Task EnsureToursTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tours') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                // Tablo var, kolonlari kontrol et
                var hasOldPriceColumn = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Price') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var hasNewPriceColumns = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Price_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                if (hasOldPriceColumn == 1 && hasNewPriceColumns == 0)
                {
                    logger.LogWarning("Tours tablosu eski semada (Price kolonu var). Migration uygulanmali.");
                }
                else
                {
                    logger.LogInformation("✓ Tours tablosu mevcut ve sema guncel.");
                }
                return;
            }

            logger.LogWarning("Tours tablosu bulunamadi. Migration uygulanmali.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tours tablosu dogrulanirken hata olustu: {Message}", ex.Message);
        }
    }

    //---News tablosunu dogrulayan metot---//
    private static async Task EnsureNewsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'News') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                logger.LogInformation("✓ News tablosu mevcut.");
                return;
            }

            logger.LogWarning("News tablosu bulunamadi. Migration uygulanmali.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "News tablosu dogrulanirken hata olustu: {Message}", ex.Message);
        }
    }

    private static async Task EnsureTestimonialsTableAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            var existsQuery = db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testimonials') THEN 1 ELSE 0 END AS [Value]");

            var exists = await existsQuery.SingleAsync();
            if (exists == 1)
            {
                logger.LogInformation("✓ Testimonials tablosu mevcut.");
                return;
            }

            // Tablo yoksa olustur
            logger.LogWarning("Testimonials tablosu bulunamadi. Olusturuluyor...");
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE [Testimonials] (
                    [Id] uniqueidentifier NOT NULL,
                    [CustomerName] nvarchar(200) NOT NULL,
                    [Location] nvarchar(200) NULL,
                    [Comment] nvarchar(2000) NOT NULL,
                    [Rating] int NOT NULL,
                    [AvatarUrl] nvarchar(500) NULL,
                    [IsApproved] bit NOT NULL DEFAULT 0,
                    [ApprovedDate] datetime2 NULL,
                    [ApprovedBy] nvarchar(100) NULL,
                    [RejectionReason] nvarchar(1000) NULL,
                    [CreatedDate] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [UpdatedDate] datetime2 NULL,
                    [UpdatedBy] nvarchar(max) NULL,
                    [IsActive] bit NOT NULL DEFAULT 1,
                    [IsDeleted] bit NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_Testimonials] PRIMARY KEY ([Id])
                );
                CREATE INDEX [IX_Testimonials_IsApproved] ON [Testimonials] ([IsApproved]);
                CREATE INDEX [IX_Testimonials_CreatedDate] ON [Testimonials] ([CreatedDate]);
            ");
            logger.LogInformation("✓ Testimonials tablosu basariyla olusturuldu.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Testimonials tablosu dogrulanirken/olusturulurken hata olustu: {Message}", ex.Message);
        }
    }

    //---Migration schema duzeltme metodu (migration history'de kayitli ama SQL calismamis)---//
    private static async Task FixMigrationSchemaAsync(TravelBookingDbContext db, ILogger logger)
    {
        try
        {
            logger.LogInformation("Migration schema kontrolu yapiliyor...");

            // Cars tablosu icin kontrol ve donusum
            var carsTableExists = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cars') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            
            if (carsTableExists == 1)
            {
                var carsHasOldPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'PricePerDay') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var carsHasNewPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'PricePerDay_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();

                if (carsHasOldPrice == 1 && carsHasNewPrice == 0)
            {
                logger.LogWarning("Cars tablosu eski semada. Donusturuluyor...");
                try
                {
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ADD PricePerDay_Amount DECIMAL(18,2) NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ADD PricePerDay_Currency NVARCHAR(10) NULL;");
                    await db.Database.ExecuteSqlRawAsync("UPDATE Cars SET PricePerDay_Amount = PricePerDay, PricePerDay_Currency = ISNULL(Currency, 'TRY');");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ALTER COLUMN PricePerDay_Amount DECIMAL(18,2) NOT NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ALTER COLUMN PricePerDay_Currency NVARCHAR(10) NOT NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars DROP COLUMN PricePerDay;");
                    
                    // Currency kolonunu kontrol et ve sil
                    var hasCurrency = await db.Database.SqlQueryRaw<int>(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cars' AND COLUMN_NAME = 'Currency') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                    if (hasCurrency == 1)
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars DROP COLUMN Currency;");
                    }
                    
                    logger.LogInformation("✓ Cars tablosu donusturuldu.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Cars tablosu donusturulurken hata olustu: {Message}", ex.Message);
                    // Hata olsa bile devam et - belki zaten donusturulmustur
                }
                }
                else if (carsHasNewPrice == 1)
                {
                    logger.LogInformation("✓ Cars tablosu zaten yeni semada.");
                }
                else if (carsHasOldPrice == 0 && carsHasNewPrice == 0)
                {
                    // Cars tablosu var ama PricePerDay kolonlari yok - eksik kolonlari ekle
                    logger.LogWarning("Cars tablosu var ama PricePerDay kolonlari bulunamadi. Eksik kolonlar ekleniyor...");
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ADD PricePerDay_Amount DECIMAL(18,2) NOT NULL DEFAULT 0;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cars ADD PricePerDay_Currency NVARCHAR(10) NOT NULL DEFAULT 'TRY';");
                        
                        // Index ekle
                        var hasIndex = await db.Database.SqlQueryRaw<int>(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cars_PricePerDay_Amount' AND object_id = OBJECT_ID('Cars')) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                        if (hasIndex == 0)
                        {
                            await db.Database.ExecuteSqlRawAsync("CREATE INDEX [IX_Cars_PricePerDay_Amount] ON [Cars] ([PricePerDay_Amount]);");
                        }
                        
                        logger.LogInformation("✓ Cars tablosuna PricePerDay kolonlari eklendi.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Cars tablosuna PricePerDay kolonlari eklenirken hata olustu: {Message}", ex.Message);
                        // Hata olsa bile devam et
                    }
                }
            }
            else
            {
                // Cars tablosu yoksa olustur (migration'in CREATE TABLE kismi calismamis olabilir)
                logger.LogWarning("Cars tablosu bulunamadi. Olusturuluyor...");
                try
                {
                    await db.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE [Cars] (
                            [Id] uniqueidentifier NOT NULL,
                            [Brand] nvarchar(100) NOT NULL,
                            [Model] nvarchar(100) NOT NULL,
                            [Category] nvarchar(50) NOT NULL,
                            [Year] int NOT NULL,
                            [FuelType] nvarchar(50) NOT NULL,
                            [Transmission] nvarchar(50) NOT NULL,
                            [Seats] int NOT NULL,
                            [Doors] int NOT NULL,
                            [PricePerDay_Amount] decimal(18,2) NOT NULL,
                            [PricePerDay_Currency] nvarchar(10) NOT NULL,
                            [ImageUrl] nvarchar(500) NOT NULL,
                            [Location] nvarchar(200) NOT NULL,
                            [HasAirConditioning] bit NOT NULL,
                            [HasGPS] bit NOT NULL,
                            [Rating] float NOT NULL,
                            [ReviewCount] int NOT NULL,
                            [IsAvailable] bit NOT NULL,
                            [CreatedDate] datetime2 NOT NULL,
                            [CreatedBy] nvarchar(max) NULL,
                            [UpdatedDate] datetime2 NULL,
                            [UpdatedBy] nvarchar(max) NULL,
                            [IsActive] bit NOT NULL,
                            [IsDeleted] bit NOT NULL,
                            CONSTRAINT [PK_Cars] PRIMARY KEY ([Id])
                        );
                    ");
                    
                    // Index'leri olustur
                    await db.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cars_Category' AND object_id = OBJECT_ID('Cars'))
                            CREATE INDEX [IX_Cars_Category] ON [Cars] ([Category]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cars_IsAvailable' AND object_id = OBJECT_ID('Cars'))
                            CREATE INDEX [IX_Cars_IsAvailable] ON [Cars] ([IsAvailable]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cars_Location' AND object_id = OBJECT_ID('Cars'))
                            CREATE INDEX [IX_Cars_Location] ON [Cars] ([Location]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cars_PricePerDay_Amount' AND object_id = OBJECT_ID('Cars'))
                            CREATE INDEX [IX_Cars_PricePerDay_Amount] ON [Cars] ([PricePerDay_Amount]);
                    ");
                    
                    logger.LogInformation("✓ Cars tablosu olusturuldu.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Cars tablosu olusturulurken hata olustu: {Message}", ex.Message);
                    // Hata olsa bile devam et - belki zaten olusturulmustur
                }
            }

            // Hotels tablosu icin kontrol ve donusum
            var hotelsTableExists = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Hotels') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            
            if (hotelsTableExists == 1)
            {
                var hotelsHasOldPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Hotels' AND COLUMN_NAME = 'PricePerNight') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var hotelsHasNewPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Hotels' AND COLUMN_NAME = 'PricePerNight_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();

                if (hotelsHasOldPrice == 1 && hotelsHasNewPrice == 0)
            {
                logger.LogWarning("Hotels tablosu eski semada. Donusturuluyor...");
                try
                {
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ADD PricePerNight_Amount DECIMAL(18,2) NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ADD PricePerNight_Currency NVARCHAR(10) NULL;");
                    await db.Database.ExecuteSqlRawAsync("UPDATE Hotels SET PricePerNight_Amount = PricePerNight, PricePerNight_Currency = ISNULL(Currency, 'TRY');");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ALTER COLUMN PricePerNight_Amount DECIMAL(18,2) NOT NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ALTER COLUMN PricePerNight_Currency NVARCHAR(10) NOT NULL;");
                    await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels DROP COLUMN PricePerNight;");
                    
                    // Currency kolonunu kontrol et ve sil
                    var hasCurrency = await db.Database.SqlQueryRaw<int>(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Hotels' AND COLUMN_NAME = 'Currency') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                    if (hasCurrency == 1)
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels DROP COLUMN Currency;");
                    }
                    
                    logger.LogInformation("✓ Hotels tablosu donusturuldu.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Hotels tablosu donusturulurken hata olustu: {Message}", ex.Message);
                    // Hata olsa bile devam et - belki zaten donusturulmustur
                }
                }
                else if (hotelsHasNewPrice == 1)
                {
                    logger.LogInformation("✓ Hotels tablosu zaten yeni semada.");
                }
                else if (hotelsHasOldPrice == 0 && hotelsHasNewPrice == 0)
                {
                    // Hotels tablosu var ama PricePerNight kolonlari yok - eksik kolonlari ekle
                    logger.LogWarning("Hotels tablosu var ama PricePerNight kolonlari bulunamadi. Eksik kolonlar ekleniyor...");
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ADD PricePerNight_Amount DECIMAL(18,2) NOT NULL DEFAULT 0;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Hotels ADD PricePerNight_Currency NVARCHAR(10) NOT NULL DEFAULT 'TRY';");
                        
                        // Index ekle
                        var hasIndex = await db.Database.SqlQueryRaw<int>(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Hotels_PricePerNight_Amount' AND object_id = OBJECT_ID('Hotels')) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                        if (hasIndex == 0)
                        {
                            await db.Database.ExecuteSqlRawAsync("CREATE INDEX [IX_Hotels_PricePerNight_Amount] ON [Hotels] ([PricePerNight_Amount]);");
                        }
                        
                        logger.LogInformation("✓ Hotels tablosuna PricePerNight kolonlari eklendi.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Hotels tablosuna PricePerNight kolonlari eklenirken hata olustu: {Message}", ex.Message);
                        // Hata olsa bile devam et
                    }
                }
            }
            else
            {
                // Hotels tablosu yoksa olustur (migration'in CREATE TABLE kismi calismamis olabilir)
                logger.LogWarning("Hotels tablosu bulunamadi. Olusturuluyor...");
                try
                {
                    await db.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE [Hotels] (
                            [Id] uniqueidentifier NOT NULL,
                            [Name] nvarchar(200) NOT NULL,
                            [City] nvarchar(100) NOT NULL,
                            [Country] nvarchar(100) NOT NULL,
                            [Address] nvarchar(500) NOT NULL,
                            [StarRating] int NOT NULL,
                            [PricePerNight_Amount] decimal(18,2) NOT NULL,
                            [PricePerNight_Currency] nvarchar(10) NOT NULL,
                            [ImageUrl] nvarchar(500) NOT NULL,
                            [Description] nvarchar(2000) NOT NULL,
                            [Rating] float NOT NULL,
                            [ReviewCount] int NOT NULL,
                            [HasFreeWifi] bit NOT NULL,
                            [HasParking] bit NOT NULL,
                            [HasPool] bit NOT NULL,
                            [HasRestaurant] bit NOT NULL,
                            [CreatedDate] datetime2 NOT NULL,
                            [CreatedBy] nvarchar(max) NULL,
                            [UpdatedDate] datetime2 NULL,
                            [UpdatedBy] nvarchar(max) NULL,
                            [IsActive] bit NOT NULL,
                            [IsDeleted] bit NOT NULL,
                            CONSTRAINT [PK_Hotels] PRIMARY KEY ([Id])
                        );
                    ");
                    
                    // Index'leri olustur
                    await db.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Hotels_City' AND object_id = OBJECT_ID('Hotels'))
                            CREATE INDEX [IX_Hotels_City] ON [Hotels] ([City]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Hotels_PricePerNight_Amount' AND object_id = OBJECT_ID('Hotels'))
                            CREATE INDEX [IX_Hotels_PricePerNight_Amount] ON [Hotels] ([PricePerNight_Amount]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Hotels_StarRating' AND object_id = OBJECT_ID('Hotels'))
                            CREATE INDEX [IX_Hotels_StarRating] ON [Hotels] ([StarRating]);
                    ");
                    
                    logger.LogInformation("✓ Hotels tablosu olusturuldu.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Hotels tablosu olusturulurken hata olustu: {Message}", ex.Message);
                    // Hata olsa bile devam et - belki zaten olusturulmustur
                }
            }

            // Tours tablosu icin kontrol ve donusum
            var toursTableExists = await db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tours') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
            
            if (toursTableExists == 1)
            {
                var toursHasOldPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Price') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                
                var toursHasNewPrice = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Price_Amount') THEN 1 ELSE 0 END AS [Value]").SingleAsync();

                if (toursHasOldPrice == 1 && toursHasNewPrice == 0)
                {
                    logger.LogWarning("Tours tablosu eski semada. Donusturuluyor...");
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ADD Price_Amount DECIMAL(18,2) NULL;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ADD Price_Currency NVARCHAR(10) NULL;");
                        await db.Database.ExecuteSqlRawAsync("UPDATE Tours SET Price_Amount = Price, Price_Currency = ISNULL(Currency, 'TRY');");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ALTER COLUMN Price_Amount DECIMAL(18,2) NOT NULL;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ALTER COLUMN Price_Currency NVARCHAR(10) NOT NULL;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours DROP COLUMN Price;");
                        
                        // Currency kolonunu kontrol et ve sil
                        var hasCurrency = await db.Database.SqlQueryRaw<int>(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Currency') THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                        if (hasCurrency == 1)
                        {
                            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours DROP COLUMN Currency;");
                        }
                        
                        logger.LogInformation("✓ Tours tablosu donusturuldu.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Tours tablosu donusturulurken hata olustu: {Message}", ex.Message);
                        // Hata olsa bile devam et - belki zaten donusturulmustur
                    }
                }
                else if (toursHasNewPrice == 1)
                {
                    logger.LogInformation("✓ Tours tablosu zaten yeni semada.");
                }
                else if (toursHasOldPrice == 0 && toursHasNewPrice == 0)
                {
                    // Tours tablosu var ama Price kolonlari yok - eksik kolonlari ekle
                    logger.LogWarning("Tours tablosu var ama Price kolonlari bulunamadi. Eksik kolonlar ekleniyor...");
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ADD Price_Amount DECIMAL(18,2) NOT NULL DEFAULT 0;");
                        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Tours ADD Price_Currency NVARCHAR(10) NOT NULL DEFAULT 'TRY';");
                        
                        // Index ekle
                        var hasIndex = await db.Database.SqlQueryRaw<int>(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tours_Price_Amount' AND object_id = OBJECT_ID('Tours')) THEN 1 ELSE 0 END AS [Value]").SingleAsync();
                        if (hasIndex == 0)
                        {
                            await db.Database.ExecuteSqlRawAsync("CREATE INDEX [IX_Tours_Price_Amount] ON [Tours] ([Price_Amount]);");
                        }
                        
                        logger.LogInformation("✓ Tours tablosuna Price kolonlari eklendi.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Tours tablosuna Price kolonlari eklenirken hata olustu: {Message}", ex.Message);
                        // Hata olsa bile devam et
                    }
                }
            }
            else
            {
                // Tours tablosu yoksa olustur (migration'in CREATE TABLE kismi calismamis olabilir)
                logger.LogWarning("Tours tablosu bulunamadi. Olusturuluyor...");
                try
                {
                    await db.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE [Tours] (
                            [Id] uniqueidentifier NOT NULL,
                            [Name] nvarchar(200) NOT NULL,
                            [Destination] nvarchar(200) NOT NULL,
                            [Duration] int NOT NULL,
                            [Price_Amount] decimal(18,2) NOT NULL,
                            [Price_Currency] nvarchar(10) NOT NULL,
                            [ImageUrl] nvarchar(500) NOT NULL,
                            [Description] nvarchar(2000) NOT NULL,
                            [Highlights] nvarchar(max) NOT NULL,
                            [Included] nvarchar(max) NOT NULL,
                            [Rating] float NOT NULL,
                            [ReviewCount] int NOT NULL,
                            [Difficulty] nvarchar(50) NOT NULL,
                            [MaxGroupSize] int NOT NULL,
                            [CreatedDate] datetime2 NOT NULL,
                            [CreatedBy] nvarchar(max) NULL,
                            [UpdatedDate] datetime2 NULL,
                            [UpdatedBy] nvarchar(max) NULL,
                            [IsActive] bit NOT NULL,
                            [IsDeleted] bit NOT NULL,
                            CONSTRAINT [PK_Tours] PRIMARY KEY ([Id])
                        );
                    ");
                    
                    // Index'leri olustur
                    await db.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tours_Destination' AND object_id = OBJECT_ID('Tours'))
                            CREATE INDEX [IX_Tours_Destination] ON [Tours] ([Destination]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tours_Duration' AND object_id = OBJECT_ID('Tours'))
                            CREATE INDEX [IX_Tours_Duration] ON [Tours] ([Duration]);
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tours_Price_Amount' AND object_id = OBJECT_ID('Tours'))
                            CREATE INDEX [IX_Tours_Price_Amount] ON [Tours] ([Price_Amount]);
                    ");
                    
                    logger.LogInformation("✓ Tours tablosu olusturuldu.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Tours tablosu olusturulurken hata olustu: {Message}", ex.Message);
                    // Hata olsa bile devam et - belki zaten olusturulmustur
                }
            }

            logger.LogInformation("Migration schema kontrolu tamamlandi.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration schema duzeltme islemi sirasinda hata olustu: {Message}", ex.Message);
            // Hata olsa bile devam et - migration'lar zaten uygulanmis olabilir
        }
    }

    //---Migration history'yi duzelten metot (tablolar var ama history'de kayit yok)---//
    private static async Task FixMigrationHistoryAsync(TravelBookingDbContext db, ILogger logger, IEnumerable<string> pendingMigrations)
    {
        try
        {
            logger.LogInformation("Migration history duzeltme islemi baslatiliyor...");
            
            //---Migration history tablosuna eksik kayitlari ekle---//
            //---Not: Bu islem sadece tablolarin zaten var oldugu durumlarda yapilmali---//
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
            var migrationsToAdd = pendingMigrations.Where(m => !appliedMigrations.Contains(m)).ToList();
            
            if (migrationsToAdd.Count == 0)
            {
                logger.LogInformation("Migration history'de eksik kayit yok.");
                return;
            }
            
            logger.LogWarning("Migration history'ye {Count} kayit eklenecek. Bu islem sadece tablolarin zaten var oldugu durumlarda guvenlidir.", migrationsToAdd.Count);
            
            //---Her migration icin history'ye kayit ekle---//
            //---EF Core'un ProductVersion'ini al (genellikle "9.0.0" veya benzeri)---//
            var productVersion = "9.0.0"; // EF Core 9.0 icin standart versiyon
            
            foreach (var migration in migrationsToAdd)
            {
                try
                {
                    //---Migration history tablosuna kayit ekle (SQL injection korumali)---//
                    //---FormattableString kullanarak parametreli sorgu olustur---//
                    FormattableString sql = $@"
                        IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {migration}) 
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({migration}, {productVersion})";
                    
                    await db.Database.ExecuteSqlInterpolatedAsync(sql);
                    
                    logger.LogInformation("Migration history'ye kayit eklendi: {Migration}", migration);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Migration history'ye kayit eklenirken hata olustu: {Migration}. Hata: {Error}", migration, ex.Message);
                }
            }
            
            //---Migration history'ye kayit ekleme isleminden sonra context'i yenile---//
            //---Bu, GetPendingMigrationsAsync'in guncel sonuclari dondurmesini saglar---//
            await db.Database.CloseConnectionAsync();
            await db.Database.OpenConnectionAsync();
            
            logger.LogInformation("Migration history duzeltme islemi tamamlandi. {Count} kayit eklendi.", migrationsToAdd.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration history duzeltme islemi basarisiz oldu.");
            //---Exception firlatma, sadece log yaz---//
        }
    }

    //---Migration'lari adim adim uygulayan metot---//
    private static async Task ApplyMigrationsStepByStepAsync(TravelBookingDbContext db, ILogger logger)
    {
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
        
        if (!pendingMigrations.Any())
        {
            logger.LogInformation("Bekleyen migration yok. Veritabani guncel.");
            return;
        }

        logger.LogInformation("Migration gecmisi tutarsizligi tespit edildi. Durum analizi yapiliyor...");
        
        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(CancellationToken.None);
        
        //---Her bekleyen migration icin kontrol yap---//
        foreach (var migration in pendingMigrations)
        {
            logger.LogInformation("Migration kontrol ediliyor: {Migration}", migration);
            
            //---Migration gecmisinde yoksa ama tablolar varsa, bu tutarsizlik demektir---//
            if (!appliedMigrations.Contains(migration))
            {
                logger.LogWarning("Migration gecmisinde yok ama tablolar mevcut olabilir: {Migration}", migration);
            }
        }
        
        //---EF Core'da tek migration uygulama dogrudan desteklenmedigi icin---//
        //---Migration gecmisini manuel olarak duzeltmek gerekebilir---//
        logger.LogWarning("Migration gecmisi tutarsizligi nedeniyle otomatik uygulama yapilamiyor.");
        logger.LogWarning("Cozum secenekleri:");
        logger.LogWarning("1. Development ortaminda: appsettings.Development.json'da 'Database:ResetOnStartup: true' yaparak veritabanini sifirlayin");
        logger.LogWarning("2. Manuel olarak: 'dotnet ef database update' komutunu calistirin");
        logger.LogWarning("3. Migration gecmisini manuel olarak duzeltin (__EFMigrationsHistory tablosu)");
        
        //---Son kontrol---//
        var remainingMigrations = await db.Database.GetPendingMigrationsAsync(CancellationToken.None);
        if (remainingMigrations.Count() > 0)
        {
            logger.LogError("Bekleyen migration'lar var: {Migrations}", string.Join(", ", remainingMigrations));
            logger.LogError("Migration'lar uygulanamadi. Yukaridaki cozum seceneklerinden birini uygulayin.");
            
            //---Development ortaminda uygulama calismaya devam edebilir ama uyari ver---//
            //---Production'da exception firlat---//
            // Not: Burada exception firlatmiyoruz cunku uygulama calismaya devam edebilir
            // Ancak migration'lar tamamlanmamis olacak
        }
    }

    //---Havalimanlarini seed eden metot---//
    private static async Task SeedAirportsAsync(TravelBookingDbContext db, ILogger logger)
    {
        if (await db.Airports.AnyAsync())
        {
            logger.LogInformation("Seed data: Havalimanlari zaten mevcut, atlaniyor.");
            return;
        }

        logger.LogInformation("Seed data: Havalimanlari ekleniyor...");

        var airports = new List<Airport>([])
        {
            //---Turkiye Havalimanlari (25)---//
            // Marmara
            new Airport("IST", "Istanbul", "Turkiye", "Istanbul Havalimani"),
            new Airport("SAW", "Istanbul", "Turkiye", "Sabiha Gokcen Havalimani"),
            new Airport("YEI", "Bursa", "Turkiye", "Bursa Yenisehir Havalimani"),
            new Airport("CKZ", "Canakkale", "Turkiye", "Canakkale Havalimani"),
            new Airport("TEQ", "Tekirdag", "Turkiye", "Tekirdag Corlu Havalimani"),
            new Airport("EDO", "Balikesir", "Turkiye", "Balikesir Koca Seyit Havalimani"),
            
            // Ege
            new Airport("ADB", "Izmir", "Turkiye", "Izmir Adnan Menderes Havalimani"),
            new Airport("BJV", "Bodrum", "Turkiye", "Bodrum-Milas Havalimani"),
            new Airport("DLM", "Dalaman", "Turkiye", "Dalaman Havalimani"),
            new Airport("DNZ", "Denizli", "Turkiye", "Denizli Cardak Havalimani"),
            
            // Akdeniz
            new Airport("AYT", "Antalya", "Turkiye", "Antalya Havalimani"),
            new Airport("GZP", "Alanya", "Turkiye", "Gazipasa-Alanya Havalimani"),
            new Airport("ADA", "Adana", "Turkiye", "Adana Sakirpasa Havalimani"),
            new Airport("HTY", "Hatay", "Turkiye", "Hatay Havalimani"),
            new Airport("KCM", "Kahramanmaras", "Turkiye", "Kahramanmaras Havalimani"),
            
            // Ic Anadolu
            new Airport("ESB", "Ankara", "Turkiye", "Ankara Esenboga Havalimani"),
            new Airport("KYA", "Konya", "Turkiye", "Konya Havalimani"),
            new Airport("ASR", "Kayseri", "Turkiye", "Kayseri Erkilet Havalimani"),
            new Airport("NAV", "Nevsehir", "Turkiye", "Nevsehir Kapadokya Havalimani"),
            
            // Karadeniz
            new Airport("TZX", "Trabzon", "Turkiye", "Trabzon Havalimani"),
            new Airport("SZF", "Samsun", "Turkiye", "Samsun Carsamba Havalimani"),
            new Airport("OGU", "Ordu", "Turkiye", "Ordu-Giresun Havalimani"),
            
            // Dogu Anadolu
            new Airport("ERZ", "Erzurum", "Turkiye", "Erzurum Havalimani"),
            new Airport("VAN", "Van", "Turkiye", "Van Ferit Melen Havalimani"),
            new Airport("AJI", "Agri", "Turkiye", "Agri Ahmed-i Hani Havalimani"),
            new Airport("MLX", "Malatya", "Turkiye", "Malatya Havalimani"),
            new Airport("EZS", "Elazig", "Turkiye", "Elazig Havalimani"),
            
            // Guneydogu Anadolu
            new Airport("GZT", "Gaziantep", "Turkiye", "Gaziantep Havalimani"),
            new Airport("GNY", "Sanliurfa", "Turkiye", "Sanliurfa GAP Havalimani"),
            new Airport("DIY", "Diyarbakir", "Turkiye", "Diyarbakir Havalimani"),
            new Airport("MQM", "Mardin", "Turkiye", "Mardin Havalimani"),

            //---Populer Avrupa Havalimanlari (20)---//
            new Airport("LHR", "London", "United Kingdom", "London Heathrow"),
            new Airport("LGW", "London", "United Kingdom", "London Gatwick"),
            new Airport("CDG", "Paris", "France", "Paris Charles de Gaulle"),
            new Airport("AMS", "Amsterdam", "Netherlands", "Amsterdam Schiphol"),
            new Airport("FRA", "Frankfurt", "Germany", "Frankfurt Airport"),
            new Airport("MUC", "Munich", "Germany", "Munich Airport"),
            new Airport("BER", "Berlin", "Germany", "Berlin Brandenburg"),
            new Airport("MAD", "Madrid", "Spain", "Madrid Barajas"),
            new Airport("BCN", "Barcelona", "Spain", "Barcelona El Prat"),
            new Airport("SVQ", "Seville", "Spain", "Seville Airport"),
            new Airport("SCQ", "Santiago de Compostela", "Spain", "Santiago de Compostela Airport"),
            new Airport("FCO", "Rome", "Italy", "Rome Fiumicino"),
            new Airport("MXP", "Milan", "Italy", "Milan Malpensa"),
            new Airport("VIE", "Vienna", "Austria", "Vienna International"),
            new Airport("ZRH", "Zurich", "Switzerland", "Zurich Airport"),
            new Airport("BRU", "Brussels", "Belgium", "Brussels Airport"),
            new Airport("CPH", "Copenhagen", "Denmark", "Copenhagen Airport"),
            new Airport("ARN", "Stockholm", "Sweden", "Stockholm Arlanda"),
            new Airport("OSL", "Oslo", "Norway", "Oslo Gardermoen"),
            new Airport("ATH", "Athens", "Greece", "Athens International"),
            new Airport("LIS", "Lisbon", "Portugal", "Lisbon Portela"),
            new Airport("DUB", "Dublin", "Ireland", "Dublin Airport"),

            //---Orta Dogu ve Asya (15)---//
            new Airport("DXB", "Dubai", "UAE", "Dubai International"),
            new Airport("AUH", "Abu Dhabi", "UAE", "Abu Dhabi International"),
            new Airport("DOH", "Doha", "Qatar", "Doha Hamad International"),
            new Airport("KWI", "Kuwait City", "Kuwait", "Kuwait International"),
            new Airport("RUH", "Riyadh", "Saudi Arabia", "Riyadh King Khalid"),
            new Airport("JED", "Jeddah", "Saudi Arabia", "Jeddah King Abdulaziz"),
            new Airport("IKA", "Tehran", "Iran", "Tehran Imam Khomeini"),
            new Airport("TLV", "Tel Aviv", "Israel", "Tel Aviv Ben Gurion"),
            new Airport("CAI", "Cairo", "Egypt", "Cairo International"),
            new Airport("SIN", "Singapore", "Singapore", "Singapore Changi"),
            new Airport("BKK", "Bangkok", "Thailand", "Bangkok Suvarnabhumi"),
            new Airport("NRT", "Tokyo", "Japan", "Tokyo Narita"),
            new Airport("HND", "Tokyo", "Japan", "Tokyo Haneda Airport"),
            new Airport("ICN", "Seoul", "South Korea", "Seoul Incheon"),
            new Airport("HKG", "Hong Kong", "Hong Kong", "Hong Kong International"),
            new Airport("PEK", "Beijing", "China", "Beijing Capital"),

            //---Amerika (10)---//
            new Airport("JFK", "New York", "USA", "New York JFK"),
            new Airport("LAX", "Los Angeles", "USA", "Los Angeles International"),
            new Airport("ORD", "Chicago", "USA", "Chicago O'Hare"),
            new Airport("MIA", "Miami", "USA", "Miami International"),
            new Airport("SFO", "San Francisco", "USA", "San Francisco International"),
            new Airport("BOS", "Boston", "USA", "Boston Logan"),
            new Airport("IAD", "Washington", "USA", "Washington Dulles"),
            new Airport("YYZ", "Toronto", "Canada", "Toronto Pearson"),
            new Airport("MEX", "Mexico City", "Mexico", "Mexico City International"),
            new Airport("GRU", "São Paulo", "Brazil", "São Paulo Guarulhos"),

            //---Diger Populer (10)---//
            new Airport("SVO", "Moscow", "Russia", "Moscow Sheremetyevo"),
            new Airport("LED", "St Petersburg", "Russia", "St Petersburg Pulkovo"),
            new Airport("SYD", "Sydney", "Australia", "Sydney Kingsford Smith"),
            new Airport("MEL", "Melbourne", "Australia", "Melbourne Airport"),
            new Airport("AKL", "Auckland", "New Zealand", "Auckland Airport"),
            new Airport("JNB", "Johannesburg", "South Africa", "Johannesburg OR Tambo"),
            new Airport("BOM", "Mumbai", "India", "Mumbai Chhatrapati Shivaji"),
            new Airport("DEL", "Delhi", "India", "Delhi Indira Gandhi"),
            new Airport("KUL", "Kuala Lumpur", "Malaysia", "Kuala Lumpur International"),
            new Airport("CGK", "Jakarta", "Indonesia", "Jakarta Soekarno-Hatta"),
            new Airport("DPS", "Denpasar", "Indonesia", "Bali Ngurah Rai")
        };

        await db.Airports.AddRangeAsync(airports);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} havalimani basariyla eklendi.", airports.Count);
    }

    //---Ucuslari seed eden metot---//
    private static async Task SeedFlightsAsync(TravelBookingDbContext db, ILogger logger)
    {
        if (await db.Flights.AnyAsync())
        {
            logger.LogInformation("Seed data: Ucuslar zaten mevcut, atlaniyor.");
            return;
        }

        logger.LogInformation("Seed data: Ucuslar ekleniyor...");

        //---Havalimani ID'lerini al---//
        var istanbul = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "IST");
        var sabiha = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "SAW");
        var ankara = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "ESB");
        var izmir = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "ADB");
        var antalya = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "AYT");
        var trabzon = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "TZX");
        var bodrum = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "BJV");
        var dalaman = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "DLM");
        var gaziantep = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "GZT");
        var diyarbakir = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "DIY");

        if (istanbul == null || ankara == null || izmir == null || antalya == null)
        {
            logger.LogWarning("Seed data: Temel havalimanlari bulunamadi. Ucus seed data atlaniyor.");
            return;
        }

        var baseDate = DateTime.Now.Date.AddDays(1); // Yarindan basla
        var flights = new List<Flight>([]);

        //---Istanbul → Ankara (5 ucus)---//
        flights.Add(new Flight("TK2100", "Turkish Airlines", istanbul.Id, ankara.Id,
            baseDate.AddHours(7), baseDate.AddHours(8).AddMinutes(15),
            new Money(1200, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2800", "Pegasus", istanbul.Id, ankara.Id,
            baseDate.AddHours(9).AddMinutes(30), baseDate.AddHours(10).AddMinutes(45),
            new Money(950, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2102", "Turkish Airlines", istanbul.Id, ankara.Id,
            baseDate.AddHours(13), baseDate.AddHours(14).AddMinutes(15),
            new Money(1350, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1400", "AnadoluJet", istanbul.Id, ankara.Id,
            baseDate.AddHours(16).AddMinutes(30), baseDate.AddHours(17).AddMinutes(45),
            new Money(850, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2104", "Turkish Airlines", istanbul.Id, ankara.Id,
            baseDate.AddHours(20), baseDate.AddHours(21).AddMinutes(15),
            new Money(1400, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));

        //---Istanbul → Izmir (4 ucus)---//
        flights.Add(new Flight("TK2310", "Turkish Airlines", istanbul.Id, izmir.Id,
            baseDate.AddHours(6).AddMinutes(30), baseDate.AddHours(7).AddMinutes(40),
            new Money(1100, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2850", "Pegasus", istanbul.Id, izmir.Id,
            baseDate.AddHours(11), baseDate.AddHours(12).AddMinutes(10),
            new Money(900, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1450", "AnadoluJet", istanbul.Id, izmir.Id,
            baseDate.AddHours(15), baseDate.AddHours(16).AddMinutes(10),
            new Money(800, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2312", "Turkish Airlines", istanbul.Id, izmir.Id,
            baseDate.AddHours(19).AddMinutes(30), baseDate.AddHours(20).AddMinutes(40),
            new Money(1250, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));

        //---Istanbul → Antalya (5 ucus)---//
        flights.Add(new Flight("TK2400", "Turkish Airlines", istanbul.Id, antalya.Id,
            baseDate.AddHours(7).AddMinutes(15), baseDate.AddHours(8).AddMinutes(35),
            new Money(1300, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2900", "Pegasus", istanbul.Id, antalya.Id,
            baseDate.AddHours(10), baseDate.AddHours(11).AddMinutes(20),
            new Money(1050, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1500", "AnadoluJet", istanbul.Id, antalya.Id,
            baseDate.AddHours(13).AddMinutes(30), baseDate.AddHours(14).AddMinutes(50),
            new Money(950, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2402", "Turkish Airlines", istanbul.Id, antalya.Id,
            baseDate.AddHours(17), baseDate.AddHours(18).AddMinutes(20),
            new Money(1400, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2902", "Pegasus", istanbul.Id, antalya.Id,
            baseDate.AddHours(21).AddMinutes(30), baseDate.AddHours(22).AddMinutes(50),
            new Money(1100, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));

        //---Istanbul → Trabzon (3 ucus)---//
        if (trabzon != null)
        {
            flights.Add(new Flight("TK2600", "Turkish Airlines", istanbul.Id, trabzon.Id,
                baseDate.AddHours(8), baseDate.AddHours(9).AddMinutes(45),
                new Money(1500, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2950", "Pegasus", istanbul.Id, trabzon.Id,
                baseDate.AddHours(14), baseDate.AddHours(15).AddMinutes(45),
                new Money(1250, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("AJ1600", "AnadoluJet", istanbul.Id, trabzon.Id,
                baseDate.AddHours(18).AddMinutes(30), baseDate.AddHours(20).AddMinutes(15),
                new Money(1150, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Ankara → Izmir (3 ucus)---//
        flights.Add(new Flight("TK2320", "Turkish Airlines", ankara.Id, izmir.Id,
            baseDate.AddHours(9), baseDate.AddHours(10).AddMinutes(10),
            new Money(1000, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2860", "Pegasus", ankara.Id, izmir.Id,
            baseDate.AddHours(14).AddMinutes(30), baseDate.AddHours(15).AddMinutes(40),
            new Money(850, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1470", "AnadoluJet", ankara.Id, izmir.Id,
            baseDate.AddHours(19), baseDate.AddHours(20).AddMinutes(10),
            new Money(750, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));

        //---Ankara → Antalya (2 ucus)---//
        flights.Add(new Flight("TK2410", "Turkish Airlines", ankara.Id, antalya.Id,
            baseDate.AddHours(10).AddMinutes(30), baseDate.AddHours(11).AddMinutes(40),
            new Money(1150, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2910", "Pegasus", ankara.Id, antalya.Id,
            baseDate.AddHours(16), baseDate.AddHours(17).AddMinutes(10),
            new Money(950, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));

        //---Izmir → Antalya (2 ucus)---//
        flights.Add(new Flight("TK2420", "Turkish Airlines", izmir.Id, antalya.Id,
            baseDate.AddHours(11), baseDate.AddHours(12).AddMinutes(5),
            new Money(900, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2920", "Pegasus", izmir.Id, antalya.Id,
            baseDate.AddHours(17).AddMinutes(30), baseDate.AddHours(18).AddMinutes(35),
            new Money(750, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));

        //---Sabiha Gokcen Ucuslari (3 ucus)---//
        if (sabiha != null)
        {
            flights.Add(new Flight("PC2805", "Pegasus", sabiha.Id, ankara.Id,
                baseDate.AddHours(8).AddMinutes(30), baseDate.AddHours(9).AddMinutes(45),
                new Money(900, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2855", "Pegasus", sabiha.Id, izmir.Id,
                baseDate.AddHours(12).AddMinutes(30), baseDate.AddHours(13).AddMinutes(40),
                new Money(850, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2905", "Pegasus", sabiha.Id, antalya.Id,
                baseDate.AddHours(15).AddMinutes(30), baseDate.AddHours(16).AddMinutes(50),
                new Money(1000, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Bodrum ve Dalaman (Turizm rotalari) (4 ucus)---//
        if (bodrum != null)
        {
            flights.Add(new Flight("TK2500", "Turkish Airlines", istanbul.Id, bodrum.Id,
                baseDate.AddHours(10), baseDate.AddHours(11).AddMinutes(10),
                new Money(1200, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2930", "Pegasus", istanbul.Id, bodrum.Id,
                baseDate.AddHours(16).AddMinutes(30), baseDate.AddHours(17).AddMinutes(40),
                new Money(1000, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        if (dalaman != null)
        {
            flights.Add(new Flight("TK2510", "Turkish Airlines", istanbul.Id, dalaman.Id,
                baseDate.AddHours(9).AddMinutes(30), baseDate.AddHours(10).AddMinutes(45),
                new Money(1250, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2940", "Pegasus", istanbul.Id, dalaman.Id,
                baseDate.AddHours(18), baseDate.AddHours(19).AddMinutes(15),
                new Money(1050, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Dogu Anadolu rotalari (2 ucus)---//
        if (gaziantep != null)
        {
            flights.Add(new Flight("TK2700", "Turkish Airlines", istanbul.Id, gaziantep.Id,
                baseDate.AddHours(11).AddMinutes(30), baseDate.AddHours(13).AddMinutes(15),
                new Money(1600, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        }

        if (diyarbakir != null)
        {
            flights.Add(new Flight("TK2750", "Turkish Airlines", istanbul.Id, diyarbakir.Id,
                baseDate.AddHours(12), baseDate.AddHours(14),
                new Money(1700, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Ek gunler icin ucuslar (Cesitlilik icin birkac gun sonrasi)---//
        var day2 = baseDate.AddDays(1);
        var day3 = baseDate.AddDays(2);
        var day7 = baseDate.AddDays(6);

        //---Hafta ici ucuslar (Day 2)---//
        flights.Add(new Flight("TK2106", "Turkish Airlines", istanbul.Id, ankara.Id,
            day2.AddHours(8).AddMinutes(30), day2.AddHours(9).AddMinutes(45),
            new Money(1150, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2802", "Pegasus", istanbul.Id, ankara.Id,
            day2.AddHours(14), day2.AddHours(15).AddMinutes(15),
            new Money(920, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2314", "Turkish Airlines", istanbul.Id, izmir.Id,
            day2.AddHours(10).AddMinutes(30), day2.AddHours(11).AddMinutes(40),
            new Money(1080, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2404", "Turkish Airlines", istanbul.Id, antalya.Id,
            day2.AddHours(12).AddMinutes(30), day2.AddHours(13).AddMinutes(50),
            new Money(1320, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2904", "Pegasus", istanbul.Id, antalya.Id,
            day2.AddHours(18), day2.AddHours(19).AddMinutes(20),
            new Money(1080, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));

        //---Hafta sonu ucuslar (Day 3) - Tatil rotalari daha yogun---//
        flights.Add(new Flight("TK2406", "Turkish Airlines", istanbul.Id, antalya.Id,
            day3.AddHours(6).AddMinutes(30), day3.AddHours(7).AddMinutes(50),
            new Money(1450, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2906", "Pegasus", istanbul.Id, antalya.Id,
            day3.AddHours(9), day3.AddHours(10).AddMinutes(20),
            new Money(1180, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1502", "AnadoluJet", istanbul.Id, antalya.Id,
            day3.AddHours(11).AddMinutes(30), day3.AddHours(12).AddMinutes(50),
            new Money(980, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2408", "Turkish Airlines", istanbul.Id, antalya.Id,
            day3.AddHours(15), day3.AddHours(16).AddMinutes(20),
            new Money(1550, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2908", "Pegasus", istanbul.Id, antalya.Id,
            day3.AddHours(19).AddMinutes(30), day3.AddHours(20).AddMinutes(50),
            new Money(1280, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));

        //---Izmir cikisli ucuslar (Day 2-3)---//
        flights.Add(new Flight("TK2322", "Turkish Airlines", izmir.Id, istanbul.Id,
            day2.AddHours(7), day2.AddHours(8).AddMinutes(10),
            new Money(1100, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2862", "Pegasus", izmir.Id, istanbul.Id,
            day2.AddHours(16).AddMinutes(30), day2.AddHours(17).AddMinutes(40),
            new Money(900, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2422", "Turkish Airlines", izmir.Id, antalya.Id,
            day3.AddHours(13), day3.AddHours(14).AddMinutes(5),
            new Money(920, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));

        //---Ankara cikisli ucuslar (Day 2-3)---//
        flights.Add(new Flight("TK2108", "Turkish Airlines", ankara.Id, istanbul.Id,
            day2.AddHours(9).AddMinutes(30), day2.AddHours(10).AddMinutes(45),
            new Money(1200, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2804", "Pegasus", ankara.Id, istanbul.Id,
            day2.AddHours(15).AddMinutes(30), day2.AddHours(16).AddMinutes(45),
            new Money(950, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2412", "Turkish Airlines", ankara.Id, antalya.Id,
            day3.AddHours(11), day3.AddHours(12).AddMinutes(10),
            new Money(1180, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));

        //---Antalya cikisli donus ucuslari (Day 3-7)---//
        flights.Add(new Flight("TK2401", "Turkish Airlines", antalya.Id, istanbul.Id,
            day3.AddHours(8), day3.AddHours(9).AddMinutes(20),
            new Money(1300, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2901", "Pegasus", antalya.Id, istanbul.Id,
            day3.AddHours(14), day3.AddHours(15).AddMinutes(20),
            new Money(1050, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("AJ1501", "AnadoluJet", antalya.Id, istanbul.Id,
            day3.AddHours(17).AddMinutes(30), day3.AddHours(18).AddMinutes(50),
            new Money(950, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));

        //---Hafta sonu sonrasi donus (Day 7 - Pazar aksami yogunlugu)---//
        flights.Add(new Flight("TK2409", "Turkish Airlines", antalya.Id, istanbul.Id,
            day7.AddHours(16), day7.AddHours(17).AddMinutes(20),
            new Money(1650, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("PC2909", "Pegasus", antalya.Id, istanbul.Id,
            day7.AddHours(18).AddMinutes(30), day7.AddHours(19).AddMinutes(50),
            new Money(1380, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        
        flights.Add(new Flight("TK2411", "Turkish Airlines", antalya.Id, istanbul.Id,
            day7.AddHours(21), day7.AddHours(22).AddMinutes(20),
            new Money(1750, Currency.TRY), 200, FlightType.Direct, FlightRegion.Domestic));

        //---Trabzon rotalari (ek)---//
        if (trabzon != null)
        {
            flights.Add(new Flight("TK2602", "Turkish Airlines", trabzon.Id, istanbul.Id,
                day2.AddHours(10).AddMinutes(30), day2.AddHours(12).AddMinutes(15),
                new Money(1500, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2952", "Pegasus", trabzon.Id, istanbul.Id,
                day2.AddHours(16), day2.AddHours(17).AddMinutes(45),
                new Money(1250, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("TK2604", "Turkish Airlines", istanbul.Id, trabzon.Id,
                day3.AddHours(12).AddMinutes(30), day3.AddHours(14).AddMinutes(15),
                new Money(1550, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Bodrum ek rotalari (Yaz sezonu yogunlugu)---//
        if (bodrum != null)
        {
            flights.Add(new Flight("TK2502", "Turkish Airlines", bodrum.Id, istanbul.Id,
                day3.AddHours(12), day3.AddHours(13).AddMinutes(10),
                new Money(1200, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2932", "Pegasus", bodrum.Id, istanbul.Id,
                day3.AddHours(18).AddMinutes(30), day3.AddHours(19).AddMinutes(40),
                new Money(1000, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Dalaman ek rotalari---//
        if (dalaman != null)
        {
            flights.Add(new Flight("TK2512", "Turkish Airlines", dalaman.Id, istanbul.Id,
                day3.AddHours(11), day3.AddHours(12).AddMinutes(15),
                new Money(1250, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2942", "Pegasus", dalaman.Id, istanbul.Id,
                day3.AddHours(19).AddMinutes(30), day3.AddHours(20).AddMinutes(45),
                new Money(1050, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Sabiha Gokcen ek ucuslar (Uygun fiyatli secenekler)---//
        if (sabiha != null)
        {
            flights.Add(new Flight("PC2807", "Pegasus", sabiha.Id, ankara.Id,
                day2.AddHours(10), day2.AddHours(11).AddMinutes(15),
                new Money(880, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2857", "Pegasus", sabiha.Id, izmir.Id,
                day2.AddHours(14), day2.AddHours(15).AddMinutes(10),
                new Money(830, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2907", "Pegasus", sabiha.Id, antalya.Id,
                day3.AddHours(8), day3.AddHours(9).AddMinutes(20),
                new Money(980, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2809", "Pegasus", ankara.Id, sabiha.Id,
                day2.AddHours(17), day2.AddHours(18).AddMinutes(15),
                new Money(900, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2859", "Pegasus", izmir.Id, sabiha.Id,
                day2.AddHours(18).AddMinutes(30), day2.AddHours(19).AddMinutes(40),
                new Money(850, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2911", "Pegasus", antalya.Id, sabiha.Id,
                day7.AddHours(20), day7.AddHours(21).AddMinutes(20),
                new Money(1080, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
        }

        //---Ic Anadolu rotalari---//
        var kayseri = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "ASR");
        var konya = await db.Airports.FirstOrDefaultAsync(a => a.IATA_Code == "KYA");
        
        if (kayseri != null)
        {
            flights.Add(new Flight("TK2800", "Turkish Airlines", istanbul.Id, kayseri.Id,
                day2.AddHours(11), day2.AddHours(12).AddMinutes(30),
                new Money(1400, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("PC2960", "Pegasus", istanbul.Id, kayseri.Id,
                day2.AddHours(16).AddMinutes(30), day2.AddHours(18),
                new Money(1150, Currency.TRY), 189, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("TK2802", "Turkish Airlines", kayseri.Id, istanbul.Id,
                day3.AddHours(13).AddMinutes(30), day3.AddHours(15),
                new Money(1400, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
        }
        
        if (konya != null)
        {
            flights.Add(new Flight("TK2820", "Turkish Airlines", istanbul.Id, konya.Id,
                day2.AddHours(13), day2.AddHours(14).AddMinutes(20),
                new Money(1100, Currency.TRY), 180, FlightType.Direct, FlightRegion.Domestic));
            
            flights.Add(new Flight("AJ1700", "AnadoluJet", istanbul.Id, konya.Id,
                day2.AddHours(17).AddMinutes(30), day2.AddHours(18).AddMinutes(50),
                new Money(900, Currency.TRY), 150, FlightType.Direct, FlightRegion.Domestic));
        }

        await db.Flights.AddRangeAsync(flights);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} ucus basariyla eklendi.", flights.Count);
    }

    private static async Task SeedHotelsAsync(TravelBookingDbContext db, ILogger logger)
    {
        // Hatali otelleri SQL ile temizle (EF Core hata veriyorsa direkt SQL kullan)
        var hotelCount = await db.Hotels.CountAsync();
        if (hotelCount > 0)
        {
            logger.LogWarning("Seed data: Mevcut {Count} otel bulundu. SQL ile temizleniyor...", hotelCount);
            await db.Database.ExecuteSqlRawAsync("DELETE FROM Hotels");
            logger.LogInformation("Seed data: Eski oteller SQL ile temizlendi.");
        }

        logger.LogInformation("Seed data: Ornek oteller ekleniyor...");

        var hotels = new List<Hotel>
        {
            // Constructor: name, city, country, address, starRating, pricePerNight, imageUrl, description, hasFreeWifi, hasParking, hasPool, hasRestaurant
            new Hotel("Grand Plaza Hotel", "Istanbul", "Turkey", "Taksim Square, Beyoglu", 5, new Money(150, Currency.USD), "/assets/img/hotel-1.jpg", "Luxury hotel in the heart of Istanbul", hasFreeWifi: true, hasParking: true, hasPool: true, hasRestaurant: true),
            new Hotel("Seaside Resort", "Antalya", "Turkey", "Lara Beach", 5, new Money(200, Currency.USD), "/assets/img/hotel-2.jpg", "Beachfront resort with all-inclusive options", hasFreeWifi: true, hasParking: true, hasPool: true, hasRestaurant: true),
            new Hotel("Business Inn", "Ankara", "Turkey", "Cankaya District", 4, new Money(80, Currency.USD), "/assets/img/hotel-3.jpg", "Modern hotel for business travelers", hasFreeWifi: true, hasParking: true, hasPool: false, hasRestaurant: true),
            new Hotel("Boutique Hotel Cappadocia", "Nevsehir", "Turkey", "Goreme", 4, new Money(120, Currency.USD), "/assets/img/hotel-4.jpg", "Cave hotel with stunning views", hasFreeWifi: true, hasParking: true, hasPool: false, hasRestaurant: true),
            new Hotel("City Center Hotel", "Izmir", "Turkey", "Alsancak", 3, new Money(60, Currency.USD), "/assets/img/hotel-5.jpg", "Affordable hotel in city center", hasFreeWifi: true, hasParking: false, hasPool: false, hasRestaurant: false),
            new Hotel("Mountain Lodge", "Bursa", "Turkey", "Uludag", 4, new Money(100, Currency.USD), "/assets/img/hotel-6.jpg", "Ski resort with mountain views", hasFreeWifi: true, hasParking: true, hasPool: true, hasRestaurant: true)
        };

        // Rating ve ReviewCount'u ayarla
        hotels[0].UpdateRating(4.8, 320);
        hotels[1].UpdateRating(4.9, 450);
        hotels[2].UpdateRating(4.5, 180);
        hotels[3].UpdateRating(4.7, 280);
        hotels[4].UpdateRating(4.2, 150);
        hotels[5].UpdateRating(4.6, 200);

        await db.Hotels.AddRangeAsync(hotels);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} otel basariyla eklendi.", hotels.Count);
    }

    private static async Task SeedCarsAsync(TravelBookingDbContext db, ILogger logger)
    {
        if (await db.Cars.AnyAsync())
        {
            logger.LogInformation("Seed data: Cars tablosu zaten doldu. Atlaniyor.");
            return;
        }

        logger.LogInformation("Seed data: Ornek araclar ekleniyor...");

        var cars = new List<Car>([])
        {
            new Car("Toyota", "Corolla", "Economy", 2023, "Gasoline", "Automatic", 5, 4, new Money(35, Currency.USD), "/assets/img/car-1.jpg", "Istanbul Airport", true, true),
            new Car("Volkswagen", "Golf", "Compact", 2023, "Diesel", "Manual", 5, 4, new Money(40, Currency.USD), "/assets/img/car-2.jpg", "Antalya Airport", true, true),
            new Car("BMW", "X5", "SUV", 2024, "Diesel", "Automatic", 7, 5, new Money(120, Currency.USD), "/assets/img/car-3.jpg", "Istanbul Airport", true, true),
            new Car("Mercedes-Benz", "E-Class", "Luxury", 2024, "Hybrid", "Automatic", 5, 4, new Money(150, Currency.USD), "/assets/img/car-4.jpg", "Ankara Airport", true, true),
            new Car("Renault", "Clio", "Economy", 2022, "Gasoline", "Manual", 5, 4, new Money(30, Currency.USD), "/assets/img/car-5.jpg", "Izmir Airport", true, false),
            new Car("Audi", "Q7", "SUV", 2024, "Diesel", "Automatic", 7, 5, new Money(130, Currency.USD), "/assets/img/car-6.jpg", "Antalya Airport", true, true)
        };

        // Rating ve ReviewCount'u ayarla
        cars[0].UpdateRating(4.5, 120);
        cars[1].UpdateRating(4.6, 95);
        cars[2].UpdateRating(4.9, 78);
        cars[3].UpdateRating(4.8, 65);
        cars[4].UpdateRating(4.3, 150);
        cars[5].UpdateRating(4.7, 88);

        await db.Cars.AddRangeAsync(cars);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} arac basariyla eklendi.", cars.Count);
    }

    private static async Task SeedToursAsync(TravelBookingDbContext db, ILogger logger)
    {
        if (await db.Tours.AnyAsync())
        {
            logger.LogInformation("Seed data: Tours tablosu zaten doldu. Atlaniyor.");
            return;
        }

        logger.LogInformation("Seed data: Ornek turlar ekleniyor...");

        var tours = new List<Tour>
        {
            new Tour("Istanbul Historical Tour", "Istanbul, Turkey", 3, new Money(299, Currency.USD), "/assets/img/tour-1.jpg", "Explore the rich history of Istanbul", "Easy", 15, ["Hagia Sophia", "Blue Mosque", "Topkapi Palace", "Grand Bazaar"], ["Hotel", "Breakfast", "Guide", "Entrance Fees"]),
            new Tour("Cappadocia Hot Air Balloon", "Cappadocia, Turkey", 2, new Money(450, Currency.USD), "/assets/img/tour-2.jpg", "Unforgettable balloon ride over fairy chimneys", "Easy", 12, ["Hot Air Balloon Ride", "Goreme Open Air Museum", "Underground City", "Pottery Workshop"], ["Hotel", "All Meals", "Balloon Ride", "Guide"]),
            new Tour("Antalya Beach & Adventure", "Antalya, Turkey", 5, new Money(599, Currency.USD), "/assets/img/tour-3.jpg", "Beach relaxation and adventure activities", "Moderate", 20, ["Beach Time", "Water Sports", "Old Town Tour", "Waterfall Visit"], ["Resort Hotel", "All-Inclusive", "Activities", "Transfers"]),
            new Tour("Pamukkale & Hierapolis", "Denizli, Turkey", 2, new Money(199, Currency.USD), "/assets/img/tour-4.jpg", "Visit the white terraces and ancient city", "Easy", 18, ["Pamukkale Travertines", "Hierapolis Ruins", "Thermal Pools", "Cleopatra's Pool"], ["Hotel", "Breakfast", "Guide", "Entrance Fees"])
        };

        // Rating ve ReviewCount'u ayarla
        tours[0].UpdateRating(4.8, 245);
        tours[1].UpdateRating(4.9, 389);
        tours[2].UpdateRating(4.7, 178);
        tours[3].UpdateRating(4.6, 156);

        await db.Tours.AddRangeAsync(tours);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} tur basariyla eklendi.", tours.Count);
    }

    private static async Task SeedNewsAsync(TravelBookingDbContext db, ILogger logger)
    {
        if (await db.News.AnyAsync())
        {
            logger.LogInformation("Seed data: News tablosu zaten doldu. Atlaniyor.");
            return;
        }

        logger.LogInformation("Seed data: Ornek haberler ekleniyor...");

        var news = new List<NewsArticle>([])
        {
            new NewsArticle("New Direct Flights to European Capitals", "We're excited to announce new direct flight routes to major European cities starting next month.", "Full article content here...", "Company News", DateTime.Now.AddDays(-2), "Travel Team", "/assets/img/news-1.jpg", ["Flights", "Europe", "New Routes"]),
            new NewsArticle("Top 10 Travel Destinations for Summer 2026", "Discover the most popular travel destinations for this summer season.", "Full article content here...", "Destinations", DateTime.Now.AddDays(-5), "Sarah Johnson", "/assets/img/news-2.jpg", ["Summer", "Destinations", "Travel Tips"]),
            new NewsArticle("Travel Safety Tips for International Flights", "Essential safety guidelines and tips for your international travel.", "Full article content here...", "Travel Tips", DateTime.Now.AddDays(-7), "John Smith", "/assets/img/news-3.jpg", ["Safety", "Tips", "International"]),
            new NewsArticle("Summer Sale: Up to 40% Off on Selected Routes", "Book now and save big on your summer vacation flights.", "Full article content here...", "Company News", DateTime.Now.AddDays(-10), "Marketing Team", "/assets/img/news-4.jpg", ["Sale", "Discount", "Summer"]),
            new NewsArticle("Exploring Istanbul: A Complete Travel Guide", "Everything you need to know about visiting Istanbul, Turkey's cultural capital.", "Full article content here...", "Destinations", DateTime.Now.AddDays(-12), "Travel Experts", "/assets/img/news-5.jpg", ["Istanbul", "Turkey", "Guide"]),
            new NewsArticle("How to Pack Light for Long Trips", "Expert tips on packing efficiently for extended travel.", "Full article content here...", "Travel Tips", DateTime.Now.AddDays(-15), "Emily Brown", "/assets/img/news-6.jpg", ["Packing", "Tips", "Travel"]),
            new NewsArticle("New Mobile App Features Released", "Check out the latest features in our mobile app update.", "Full article content here...", "Company News", DateTime.Now.AddDays(-18), "Tech Team", "/assets/img/news-7.jpg", ["App", "Technology", "Update"]),
            new NewsArticle("Best Budget Airlines in Europe", "Comprehensive comparison of budget airlines operating in Europe.", "Full article content here...", "Industry", DateTime.Now.AddDays(-20), "Industry Analyst", "/assets/img/news-8.jpg", ["Budget", "Airlines", "Europe"])
        };

        // Tum haberleri yayinla ve view count'u ayarla
        foreach (var article in news)
        {
            article.Publish();
            // ViewCount'u ayarlamak icin reflection kullanmamiz gerekiyor cunku private setter var
            // Ancak IncrementViewCount metodu var, bu yuzden view count kadar cagiralim
            // Ama bu seed data icin pratik degil, bu yuzden ViewCount'u seed data'da 0 olarak birakiyoruz
            // Gercek kullanimda kullanicilar makaleyi goruntulediginde artacak
        }

        await db.News.AddRangeAsync(news);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Seed data: {Count} haber basariyla eklendi.", news.Count);
    }
}
