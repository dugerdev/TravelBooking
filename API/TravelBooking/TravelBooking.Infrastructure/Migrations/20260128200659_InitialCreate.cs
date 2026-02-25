using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IATA_Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, comment: "IATA Kodu (International Air Transport Association)"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Sehir"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Ulke"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Havalimani tam adi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Arac markasi"),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Arac modeli"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Kategori"),
                    Year = table.Column<int>(type: "int", nullable: false, comment: "Model yili"),
                    FuelType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Yakit tipi"),
                    Transmission = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Vites tipi"),
                    Seats = table.Column<int>(type: "int", nullable: false),
                    Doors = table.Column<int>(type: "int", nullable: false),
                    PricePerDay_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Gunluk kiralama ucreti"),
                    PricePerDay_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Gorsel URL"),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Lokasyon"),
                    HasAirConditioning = table.Column<bool>(type: "bit", nullable: false),
                    HasGPS = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false, comment: "Ortalama puan"),
                    ReviewCount = table.Column<int>(type: "int", nullable: false, comment: "Yorum sayisi"),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, comment: "Musaitlik durumu"),
                    MileagePolicy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Unlimited", comment: "Kilometre politikasi"),
                    FuelPolicy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Full to Full", comment: "Yakit politikasi"),
                    PickupLocationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "In Terminal", comment: "Teslim alma yeri tipi"),
                    Supplier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "", comment: "Tedarikci/kiralama sirketi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Otel adi"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Sehir"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Ulke"),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Adres"),
                    StarRating = table.Column<int>(type: "int", nullable: false, comment: "Yildiz sayisi"),
                    PricePerNight_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Gecelik fiyat"),
                    PricePerNight_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Gorsel URL"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Aciklama"),
                    Rating = table.Column<double>(type: "float", nullable: false, comment: "Ortalama puan"),
                    ReviewCount = table.Column<int>(type: "int", nullable: false, comment: "Yorum sayisi"),
                    HasFreeWifi = table.Column<bool>(type: "bit", nullable: false),
                    HasParking = table.Column<bool>(type: "bit", nullable: false),
                    HasPool = table.Column<bool>(type: "bit", nullable: false),
                    HasRestaurant = table.Column<bool>(type: "bit", nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Hotel", comment: "Tesis tipi"),
                    DistanceFromCenter = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0, comment: "Merkeze uzaklik (km)"),
                    SustainabilityLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Surdurulebilirlik seviyesi"),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Marka/zincir adi"),
                    Neighbourhood = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Semt/bolge"),
                    HasAirConditioning = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Klima"),
                    HasFitnessCenter = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Fitness merkezi"),
                    HasSpa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "SPA"),
                    HasBreakfast = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Kahvalti"),
                    HasFreeCancellation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Ucretsiz iptal"),
                    NoPrepaymentNeeded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "On odeme gereksiz"),
                    HasAccessibilityFeatures = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Engelli erisimi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "Haber basligi"),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "Ozet"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Icerik"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Kategori"),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Yayin tarihi"),
                    Author = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Yazar"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Gorsel URL"),
                    ViewCount = table.Column<int>(type: "int", nullable: false, comment: "Goruntulenme sayisi"),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false, comment: "Yayinda mi?"),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Etiketler (JSON)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Gonderen adi"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "E-posta"),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Telefon"),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "Konu"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Mesaj"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, comment: "Okundu mu?"),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Okunma tarihi"),
                    ReadBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Okuyan"),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Yanit"),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Yanit tarihi"),
                    ResponseBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Yanitlayan"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassengerFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Yolcu adi"),
                    PassengerLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Yolcu soyadi"),
                    NationalNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "T.C. Kimlik No veya Yabanci Kimlik No"),
                    PassportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Pasaport No"),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Dogum tarihi"),
                    PassengerType = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Yolcu Tipi (Yetiskin, Cocuk, Bebek)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Tur adi"),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Destinasyon"),
                    Duration = table.Column<int>(type: "int", nullable: false, comment: "Sure (gun)"),
                    Price_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Fiyat"),
                    Price_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Gorsel URL"),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Aciklama"),
                    Difficulty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Zorluk seviyesi"),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: false, comment: "Maksimum grup buyuklugu"),
                    Rating = table.Column<double>(type: "float", nullable: false, comment: "Ortalama puan"),
                    ReviewCount = table.Column<int>(type: "int", nullable: false, comment: "Yorum sayisi"),
                    Highlights = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "One cikan ozellikler (JSON)"),
                    Included = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Dahil olan hizmetler (JSON)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Musteri adi"),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Konum"),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Yorum"),
                    Rating = table.Column<int>(type: "int", nullable: false, comment: "Puan"),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Avatar URL"),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Onay durumu"),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Onay tarihi"),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Onaylayan"),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Red sebebi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Ucus numarasi"),
                    AirlineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Ucus sirketi adi"),
                    FlightType = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Ucus turu"),
                    FlightRegion = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Ucus bolgesi"),
                    BasePrice_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Temel bilet fiyati"),
                    BasePrice_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false, comment: "Musait koltuklar"),
                    TotalSeats = table.Column<int>(type: "int", nullable: false, comment: "Toplam koltuk sayisi"),
                    DepartureAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArrivalAirportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledDeparture = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Planlanan kalkis zamani"),
                    ScheduledArrival = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Planlanan varis zamani"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_ArrivalAirportId",
                        column: x => x.ArrivalAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DepartureAirportId",
                        column: x => x.DepartureAirportId,
                        principalTable: "Airports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PNR = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "PNR (Passenger Name Record) - Rezervasyon referans numarasi"),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false, comment: "Kullanici kimligi"),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Toplam fiyat"),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Odeme durumu"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Odeme yontemi"),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Rezervasyon durumu"),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Rezervasyon tarihi"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Rezervasyon son kullanma tarihi"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true, comment: "Concurrency kontrolu icin row version"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Oda tipi"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Oda fiyati"),
                    MaxGuests = table.Column<int>(type: "int", nullable: false, comment: "Maksimum misafir sayisi"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "Aciklama"),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Oda ozellikleri (JSON)"),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, comment: "Musaitlik durumu"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionAmount_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Islem tutari"),
                    TransactionAmount_Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Para birimi"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Odeme durumu"),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Odeme yontemi"),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Islem kimligi"),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Islem turu"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Hata mesaji"),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Islem tarihi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassengerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "E-posta adresi"),
                    ContactPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Iletisim telefon numarasi"),
                    SeatClass = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Koltuk sinifi"),
                    SeatNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, comment: "Koltuk numarasi"),
                    BaggageOption = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Bagaj secenegi"),
                    TicketPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Bilet fiyati"),
                    BaggageFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false, comment: "Bagaj ucreti"),
                    TicketStatus = table.Column<string>(type: "nvarchar(450)", nullable: false, comment: "Bilet durumu"),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Iptal edilme tarihi"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airports_City",
                table: "Airports",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_Country",
                table: "Airports",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_IATA_Code",
                table: "Airports",
                column: "IATA_Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Category",
                table: "Cars",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_IsAvailable",
                table: "Cars",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Location",
                table: "Cars",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_IsRead",
                table: "ContactMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CreatedDate",
                table: "ContactMessages",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ArrivalAirportId",
                table: "Flights",
                column: "ArrivalAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureAirportId",
                table: "Flights",
                column: "DepartureAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ScheduledDeparture",
                table: "Flights",
                column: "ScheduledDeparture");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_City",
                table: "Hotels",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_StarRating",
                table: "Hotels",
                column: "StarRating");

            migrationBuilder.CreateIndex(
                name: "IX_News_Category",
                table: "News",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_News_IsPublished",
                table: "News",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_News_PublishDate",
                table: "News",
                column: "PublishDate");

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_NationalNumber",
                table: "Passengers",
                column: "NationalNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_PassportNumber",
                table: "Passengers",
                column: "PassportNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentStatus",
                table: "Payments",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReservationId",
                table: "Payments",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionDate",
                table: "Payments",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AppUserId",
                table: "Reservations",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PaymentStatus",
                table: "Reservations",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PNR",
                table: "Reservations",
                column: "PNR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HotelId",
                table: "Rooms",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_IsAvailable",
                table: "Rooms",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FlightId",
                table: "Tickets",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PassengerId",
                table: "Tickets",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReservationId",
                table: "Tickets",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketStatus",
                table: "Tickets",
                column: "TicketStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_Destination",
                table: "Tours",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_Duration",
                table: "Tours",
                column: "Duration");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsApproved",
                table: "Testimonials",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_CreatedDate",
                table: "Testimonials",
                column: "CreatedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Testimonials");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Passengers");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
