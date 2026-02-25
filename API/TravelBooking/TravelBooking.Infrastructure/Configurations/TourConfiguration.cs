using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace TravelBooking.Infrastructure.Configurations;

public class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.ToTable("Tours");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Tur adi");

        builder.Property(t => t.Destination)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Destinasyon");

        builder.Property(t => t.Duration)
            .IsRequired()
            .HasComment("Sure (gun)");

        builder.Property(t => t.ImageUrl)
            .HasMaxLength(500)
            .HasComment("Gorsel URL");

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .HasComment("Aciklama");

        builder.Property(t => t.Difficulty)
            .HasMaxLength(50)
            .HasComment("Zorluk seviyesi");

        builder.Property(t => t.MaxGroupSize)
            .HasComment("Maksimum grup buyuklugu");

        builder.Property(t => t.Rating)
            .HasComment("Ortalama puan");

        builder.Property(t => t.ReviewCount)
            .HasComment("Yorum sayisi");

        // Money Value Object icin owned entity
        builder.OwnsOne(t => t.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Fiyat");

            money.Property(m => m.Currency)
                .HasColumnName("Price_Currency")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasComment("Para birimi");
        });

        // JSON columns for list properties (using backing fields)
        var listComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
        builder.Property(t => t.Highlights)
            .HasField("_highlights")
            .HasConversion(
                v => JsonSerializer.Serialize(v.ToList(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasComment("One cikan ozellikler (JSON)")
            .Metadata.SetValueComparer(listComparer);

        builder.Property(t => t.Included)
            .HasField("_included")
            .HasConversion(
                v => JsonSerializer.Serialize(v.ToList(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasComment("Dahil olan hizmetler (JSON)")
            .Metadata.SetValueComparer(listComparer);

        // Index'ler
        builder.HasIndex(t => t.Destination);
        builder.HasIndex(t => t.Duration);
        // Price.Amount index'i migration'da olusturuluyor (owned entity property'leri icin HasIndex kullanilamaz)
    }
}
