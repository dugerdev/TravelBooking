using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Flight entity icin Fluent API konfigurasyonlari---//
public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Flights");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(f => f.Id);

        //---Ucus ozel alanlari---//
        builder.Property(f => f.FlightNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Ucus numarasi");

        builder.Property(f => f.AirlineName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Ucus sirketi adi");

        builder.Property(f => f.FlightType)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Ucus turu");

        builder.Property(f => f.FlightRegion)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Ucus bolgesi");

        builder.Property(f => f.AvailableSeats)
            .IsRequired()
            .HasComment("Musait koltuklar");

        builder.Property(f => f.TotalSeats)
            .IsRequired()
            .HasComment("Toplam koltuk sayisi");

        builder.Property(f => f.ScheduledDeparture)
            .IsRequired()
            .HasComment("Planlanan kalkis zamani");

        builder.Property(f => f.ScheduledArrival)
            .IsRequired()
            .HasComment("Planlanan varis zamani");

        //---Money Value Object icin owned entity---//
        //---EF Core owned type'larda column name otomatik olusturulur: {PropertyName}_{SubPropertyName}---//
        builder.OwnsOne(f => f.BasePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BasePrice_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Temel bilet fiyati");

            money.Property(m => m.Currency)
                .HasColumnName("BasePrice_Currency")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasComment("Para birimi");
        });

        //---Index'ler---//
        builder.HasIndex(f => f.FlightNumber);
        builder.HasIndex(f => f.DepartureAirportId);
        builder.HasIndex(f => f.ArrivalAirportId);
        builder.HasIndex(f => f.ScheduledDeparture);

        //---Iliskiler---//
        builder.HasOne(f => f.DepartureAirport)
            .WithMany(a => a.DepartureFlights)
            .HasForeignKey(f => f.DepartureAirportId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Flights_Airports_DepartureAirportId");

        builder.HasOne(f => f.ArrivalAirport)
            .WithMany(a => a.ArrivalFlights)
            .HasForeignKey(f => f.ArrivalAirportId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Flights_Airports_ArrivalAirportId");

        builder.HasMany(f => f.Tickets)
            .WithOne(t => t.Flight)
            .HasForeignKey(t => t.FlightId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tickets_Flights_FlightId");
    }
}

