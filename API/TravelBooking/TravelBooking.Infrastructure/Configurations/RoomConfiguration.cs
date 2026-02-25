using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace TravelBooking.Infrastructure.Configurations;

/// <summary>
/// Room entity icin Fluent API konfigurasyonlari
/// </summary>
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Oda tipi");

        builder.Property(r => r.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Oda fiyati");

        builder.Property(r => r.MaxGuests)
            .IsRequired()
            .HasComment("Maksimum misafir sayisi");

        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .HasComment("Aciklama");

        // JSON column for Features list
        var listComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
        builder.Property(r => r.Features)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasComment("Oda ozellikleri (JSON)")
            .Metadata.SetValueComparer(listComparer);

        builder.Property(r => r.IsAvailable)
            .IsRequired()
            .HasComment("Musaitlik durumu");

        // Index'ler
        builder.HasIndex(r => r.HotelId);
        builder.HasIndex(r => r.IsAvailable);

        // Iliskiler
        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
