using TravelBooking.Application;
using TravelBooking.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TravelBooking.Api.Services;
using TravelBooking.Api;
using TravelBooking.Api.Services.Auth;
using TravelBooking.Api.HostedServices;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TravelBooking.Api.Authorization;
using TravelBooking.Infrastructure.Data;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

//Örnek: JWT Secret kontrolu - Production'da guclu sifre (32+ karakter) zorunlu; Development'ta default kullanilir
var jwtSecret = builder.Configuration["JWT:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) ||
    jwtSecret.Contains("CHANGE_THIS", StringComparison.OrdinalIgnoreCase) ||
    jwtSecret.Length < 32)
{
    //---Development'ta default secret kullanilacak, sadece production'da hata ver---//
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("JWT:Secret must be set to a strong secret (>= 32 chars) for non-development environments.");
    else
    {
        //---Development'ta default secret kullan---//
        builder.Configuration["JWT:Secret"] = "Development-Default-Secret-Key-At-Least-32-Characters-Long-For-JWT-Signing";
    }
}

// Controller'lari ekle ve yapilandir
// Request size limits (DoS korumasi)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30_000_000; // 30 MB
    options.ValueLengthLimit = 30_000_000;
    options.ValueCountLimit = 1000;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30_000_000; // 30 MB
});

// Bu, tum API endpoint'lerinin (Controller'larin) kaydedilmesini saglar
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Model validation'i devre disi birak (FluentValidation kullaniyoruz)
        // FluentValidation daha esnek ve guclu bir validation saglar
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(options =>
    {
        // JSON property isimlerini PascalCase olarak tut (camelCase'e cevirme)
        // C# standart naming convention'ini koruyoruz
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Enum'lari string olarak serialize et (Currency, PaymentStatus, vb.)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// FluentValidation yapilandirmasi
// Tum validator'lari otomatik olarak kaydeder ve request'leri validate eder
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(); // Otomatik validation
builder.Services.AddFluentValidationClientsideAdapters(); // Client-side validation destegi

// Response Compression (Gzip/Brotli)
// Buyuk response'lar icin network trafigini azaltir
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // HTTPS icin de compression aktif
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Swagger/OpenAPI yapilandirmasi
// API dokumantasyonu icin Swagger UI kullaniyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //---Swagger annotations'i etkinlestir---//
    options.EnableAnnotations();

    // Swagger UI'da JWT Bearer token destegi
    // Bu sayede Swagger'dan direkt olarak authenticated request'ler gonderebiliriz
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token giriniz: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // "Failed to fetch" / localhost sorunlarinda 127.0.0.1 secilebilir
    options.DocumentFilter<TravelBooking.Api.Swagger.ServersDocumentFilter>();
});

//---Caching---//
// Memory cache ekle (sik kullanilan veriler icin)
builder.Services.AddMemoryCache();

//---Persistence + Application servisleri---//
// Persistence: Veritabani baglantilari ve repository'ler
// Application: Is mantigi servisleri ve DTO'lar
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Saglik kontrolu (Health Checks)
// Uygulamanin ve veritabaninin durumunu kontrol etmek icin endpoint'ler ekler
// /health, /health/ready, /health/live endpoint'leri otomatik olusturulur
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TravelBookingDbContext>("database", tags: HealthCheckTags.Ready)
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: HealthCheckTags.SelfAndLive);

// AutoMapper yapilandirmasi
// Entity'leri DTO'lara otomatik olarak map eder (manuel mapping yapmaya gerek kalmaz)
builder.Services.AddAutoMapper(typeof(TravelBooking.Application.Mappings.MappingProfile));

// Token servisleri
// JWT token olusturma ve refresh token yonetimi icin servisler
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Background servisler (arka planda calisan gorevler)
// RefreshTokenCleanupService: Suresi dolmus refresh token'lari temizler
// FlightSyncBackgroundService: Dis API'den ucus bilgilerini senkronize eder
builder.Services.AddHostedService<RefreshTokenCleanupService>();
builder.Services.AddHostedService<FlightSyncBackgroundService>();

//Örnek: Rate Limiting - brute-force saldirilarini onlemek icin auth ve external-search endpoint'lerine istek limiti
builder.Services.AddRateLimiter(options =>
{
    // Rate limit asildiginda donecek HTTP status code
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    //Örnek: Auth policy - her IP dakikada max 20 istek (login/signup brute-force korumasi)
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20, // Dakikada 20 istek
                Window = TimeSpan.FromMinutes(1), // 1 dakikalik pencere
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // Siralama: en eskiden yeniye
                QueueLimit = 0 // Queue limit yok (direkt reddet)
            }));

    //Örnek: External search policy - dis API maliyetlerini kontrol icin dakikada 10 arama, 2 istek kuyruk
    options.AddPolicy("external-search", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Dakikada 10 arama
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2 // En fazla 2 istek siraya alinabilir
            }));
});

//Örnek: JWT Bearer kimlik dogrulama - token Issuer, Audience ve imza anahtari dogrulanir
// Bu policy'ler, kullanicilarin hangi endpoint'lere erisebilecegini kontrol eder
builder.Services.AddAuthorization(options =>
{
    Policies.ConfigurePolicies(options);
});

// Kimlik dogrulama (Authentication) yapilandirmasi
// JWT token tabanli kimlik dogrulama kullaniyoruz
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret ?? "Development-Default-Secret-Key-At-Least-32-Characters-Long-For-JWT-Signing"))
        };
    });

//Örnek: CORS - Frontend'in API'ye erismesi icin izin verilen origin'ler (AllowCredentials ile JWT cookie destegi)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        // appsettings.json'dan izin verilen origin'leri oku
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (origins.Length == 0)
        {
            // Development ortaminda Swagger UI ve localhost origin'lerine izin ver
            // AllowCredentials() icin AllowAnyOrigin() kullanilamaz, bu yuzden WithOrigins kullaniyoruz
            policy.WithOrigins("https://localhost:7283", "http://localhost:5000", "https://localhost:5001", "http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // JWT token gondermek icin gerekli
        }
        else
        {
            // Production'da sadece belirtilen origin'lere izin ver
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // JWT token gondermek icin gerekli
        }
    });
});

var app = builder.Build();

// Reverse proxy veya load balancer arkasinda calisiyorsak
// Forwarded header'lari kullanarak dogru IP ve scheme'i al
// Bu, production ortaminda onemlidir (orn: Nginx, IIS, Azure App Service)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// HTTP request pipeline yapilandirmasi
// Request/Response logging middleware (opsiyonel, config'den kontrol edilir)
app.UseMiddleware<TravelBooking.Api.Middleware.RequestResponseLoggingMiddleware>();

// Global exception handler middleware - tum hatalari yakalar ve tutarli JSON response doner
app.UseMiddleware<TravelBooking.Api.Middleware.GlobalExceptionHandlerMiddleware>();

// Development ortaminda Swagger UI'i etkinlestir
// Production'da Swagger devre disidir (guvenlik)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint'i
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelBooking API V1");
        c.RoutePrefix = "swagger"; // Swagger UI'i /swagger adresinde ac
    });
}
else
{
    // Production'da HSTS (HTTP Strict Transport Security) etkinlestir
    // Bu, tarayiciyi sadece HTTPS kullanmaya zorlar
    app.UseHsts();
}

// HTTPS yonlendirmesi
// HTTP isteklerini otomatik olarak HTTPS'e yonlendirir
app.UseHttpsRedirection();

//---Response Compression---//
// Response'lari sikistirarak network trafigini azaltir
app.UseResponseCompression();

//---Security Headers---//
// Guvenlik header'larini ekle (XSS, clickjacking, MIME sniffing korumasi)
app.Use(async (context, next) =>
{
    // Header'lari sadece yoksa ekle (zaten set edilmisse degistirme)
    if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
        context.Response.Headers.XContentTypeOptions = "nosniff";
    
    if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
        context.Response.Headers.XFrameOptions = "DENY";
    
    if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
        context.Response.Headers.XXSSProtection = "1; mode=block";
    
    if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // Content-Security-Policy (API icin minimal, Swagger haric)
    if (!context.Request.Path.StartsWithSegments("/swagger") && 
        !context.Response.Headers.ContainsKey("Content-Security-Policy"))
    {
        context.Response.Headers.ContentSecurityPolicy = "default-src 'self'";
    }
    
    await next();
});

// CORS middleware'i
// Frontend uygulamasinin API'ye erismesine izin verir
app.UseCors("CorsPolicy");

// Rate Limiter middleware'i
// Istek limitlemeyi aktif eder
app.UseRateLimiter();

// Kimlik dogrulama ve yetkilendirme middleware'leri
// Bu middleware'ler, isteklerin kimlik dogrulamasini ve yetkilendirmesini kontrol eder
app.UseAuthentication();
app.UseAuthorization();

// Saglik kontrolu endpoint'leri
// Bu endpoint'ler, uygulamanin saglik durumunu kontrol etmek icin kullanilir
// Ornegin: load balancer veya monitoring sistemleri bu endpoint'leri kontrol edebilir
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Ready check: Uygulama istekleri kabul etmeye hazir mi?
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness check: Uygulama calisiyor mu?
    Predicate = check => check.Tags.Contains("live")
});

// Controller'lari map et (tum API endpoint'leri buradan gelir)
app.MapControllers();

await DbSeeder.SeedData(app);

app.Run();
