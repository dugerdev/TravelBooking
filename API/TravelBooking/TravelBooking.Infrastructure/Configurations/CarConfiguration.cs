using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Arac markasi");

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Arac modeli");

        builder.Property(c => c.Category)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Kategori");

        builder.Property(c => c.Year)
            .IsRequired()
            .HasComment("Model yili");

        builder.Property(c => c.FuelType)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Yakit tipi");

        builder.Property(c => c.Transmission)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Vites tipi");

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500)
            .HasComment("Gorsel URL");

        builder.Property(c => c.Location)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Lokasyon");

        builder.Property(c => c.Rating)
            .HasComment("Ortalama puan");

        builder.Property(c => c.ReviewCount)
            .HasComment("Yorum sayisi");

        builder.Property(c => c.IsAvailable)
            .IsRequired()
            .HasComment("Musaitlik durumu");

        builder.Property(c => c.MileagePolicy)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Unlimited")
            .HasComment("Kilometre politikasi");

        builder.Property(c => c.FuelPolicy)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Full to Full")
            .HasComment("Yakit politikasi");

        builder.Property(c => c.PickupLocationType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("In Terminal")
            .HasComment("Teslim alma tipi");

        builder.Property(c => c.Supplier)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("")
            .HasComment("Tedarikci/Kiralama sirketi");

        // Money Value Object icin owned entity
        builder.OwnsOne(c => c.PricePerDay, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PricePerDay_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Gunluk kiralama ucreti");

            money.Property(m => m.Currency)
                .HasColumnName("PricePerDay_Currency")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasComment("Para birimi");
        });

        // Index'ler
        builder.HasIndex(c => c.Location);
        builder.HasIndex(c => c.Category);
        builder.HasIndex(c => c.IsAvailable);
        // PricePerDay.Amount index'i migration'da olusturuluyor (owned entity property'leri icin HasIndex kullanilamaz)
    }
}
