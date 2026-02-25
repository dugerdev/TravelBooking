using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Airport entity icin Fluent API konfigurasyonlari---//
public class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Airports");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(a => a.Id);

        //---Havalimani ozel alanlari---//
        builder.Property(a => a.IATA_Code)
            .IsRequired()
            .HasMaxLength(3)
            .HasComment("IATA Kodu (International Air Transport Association)");

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Sehir");

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Ulke");

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Havalimani tam adi");

        //---Index'ler---//
        builder.HasIndex(a => a.IATA_Code)
            .IsUnique()
            .HasDatabaseName("IX_Airports_IATA_Code");

        builder.HasIndex(a => a.City);
        builder.HasIndex(a => a.Country);
    }
}

