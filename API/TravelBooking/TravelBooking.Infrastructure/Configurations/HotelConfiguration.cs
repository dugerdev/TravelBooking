using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Otel adi");

        builder.Property(h => h.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Sehir");

        builder.Property(h => h.Country)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Ulke");

        builder.Property(h => h.Address)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Adres");

        builder.Property(h => h.StarRating)
            .IsRequired()
            .HasComment("Yildiz sayisi");

        builder.Property(h => h.ImageUrl)
            .HasMaxLength(500)
            .HasComment("Gorsel URL");

        builder.Property(h => h.Description)
            .HasMaxLength(2000)
            .HasComment("Aciklama");

        builder.Property(h => h.Rating)
            .HasComment("Ortalama puan");

        builder.Property(h => h.ReviewCount)
            .HasComment("Yorum sayisi");

        builder.Property(h => h.PropertyType)
            .HasMaxLength(50)
            .HasDefaultValue("Hotel")
            .HasComment("Tesis tipi");

        builder.Property(h => h.DistanceFromCenter)
            .HasDefaultValue(0)
            .HasComment("Merkeze uzaklik (km)");

        builder.Property(h => h.SustainabilityLevel)
            .HasDefaultValue(0)
            .HasComment("Surdurulebilirlik seviyesi");

        builder.Property(h => h.Brand)
            .HasMaxLength(100)
            .HasComment("Marka/zincir adi");

        builder.Property(h => h.Neighbourhood)
            .HasMaxLength(200)
            .HasComment("Semt/bolge");

        builder.Property(h => h.HasFreeWifi)
            .IsRequired()
            .HasComment("Ucretsiz WiFi");

        builder.Property(h => h.HasParking)
            .IsRequired()
            .HasComment("Otopark");

        builder.Property(h => h.HasPool)
            .IsRequired()
            .HasComment("Havuz");

        builder.Property(h => h.HasRestaurant)
            .IsRequired()
            .HasComment("Restoran");

        builder.Property(h => h.HasAirConditioning)
            .HasDefaultValue(false)
            .HasComment("Klima");

        builder.Property(h => h.HasFitnessCenter)
            .HasDefaultValue(false)
            .HasComment("Fitness merkezi");

        builder.Property(h => h.HasSpa)
            .HasDefaultValue(false)
            .HasComment("SPA");

        builder.Property(h => h.HasBreakfast)
            .HasDefaultValue(false)
            .HasComment("Kahvalti");

        builder.Property(h => h.HasFreeCancellation)
            .HasDefaultValue(false)
            .HasComment("Ucretsiz iptal");

        builder.Property(h => h.NoPrepaymentNeeded)
            .HasDefaultValue(false)
            .HasComment("On odeme gereksiz");

        builder.Property(h => h.HasAccessibilityFeatures)
            .HasDefaultValue(false)
            .HasComment("Engelli erisimi");

        // Money Value Object icin owned entity
        builder.OwnsOne(h => h.PricePerNight, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PricePerNight_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Gecelik fiyat");

            money.Property(m => m.Currency)
                .HasColumnName("PricePerNight_Currency")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasComment("Para birimi");
        });

        // Index'ler
        builder.HasIndex(h => h.City);
        builder.HasIndex(h => h.StarRating);
        // PricePerNight.Amount index'i migration'da olusturuluyor (owned entity property'leri icin HasIndex kullanilamaz)

        // Iliskiler
        builder.HasMany(h => h.Rooms)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
