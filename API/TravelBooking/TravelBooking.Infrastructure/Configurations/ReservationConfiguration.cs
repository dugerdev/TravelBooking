using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Reservation entity icin Fluent API konfigurasyonlari---//
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Reservations");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(r => r.Id);

        //---Rezervasyon ozel alanlari---//
        builder.Property(r => r.PNR)
            .IsRequired()
            .HasMaxLength(10)
            .HasComment("PNR (Passenger Name Record) - Rezervasyon referans numarasi");

        builder.Property(r => r.AppUserId)
            .IsRequired()
            .HasMaxLength(450)
            .HasComment("Kullanici kimligi");

        builder.Property(r => r.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Toplam fiyat");

        builder.Property(r => r.Currency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Para birimi");

        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Rezervasyon turu (Flight, Hotel, Car, Tour)");

        builder.Property(r => r.HotelId)
            .HasComment("Otel rezervasyonu icin otel ID");

        builder.Property(r => r.CarId)
            .HasComment("Arac kiralama rezervasyonu icin arac ID");

        builder.Property(r => r.TourId)
            .HasComment("Tur rezervasyonu icin tur ID");

        builder.Property(r => r.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Odeme durumu");

        builder.Property(r => r.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Odeme yontemi");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Rezervasyon durumu");

        builder.Property(r => r.ReservationDate)
            .IsRequired()
            .HasComment("Rezervasyon tarihi");

        builder.Property(r => r.ExpirationDate)
            .HasComment("Rezervasyon son kullanma tarihi");

        builder.Property(r => r.FlightRouteSummary)
            .HasMaxLength(200)
            .HasComment("Harici ucus guzergahi (orn. Istanbul - Antalya)");

        builder.Property(r => r.CarPickUpDate).HasComment("Arac kiralama alis tarihi");
        builder.Property(r => r.CarDropOffDate).HasComment("Arac kiralama birakis tarihi");
        builder.Property(r => r.CarPickUpLocation).HasMaxLength(200).HasComment("Arac kiralama alis yeri");
        builder.Property(r => r.CarDropOffLocation).HasMaxLength(200).HasComment("Arac kiralama birakis yeri");

        //---Concurrency kontrolu icin row version---//
        builder.Property(r => r.RowVersion)
            .IsRowVersion()
            .HasComment("Concurrency kontrolu icin row version");

        //---Index'ler---//
        builder.HasIndex(r => r.PNR)
            .IsUnique()
            .HasDatabaseName("IX_Reservations_PNR");

        builder.HasIndex(r => r.AppUserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.PaymentStatus);
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.HotelId);
        builder.HasIndex(r => r.CarId);
        builder.HasIndex(r => r.TourId);
        
        //---Composite indexes for common query patterns---//
        builder.HasIndex(r => new { r.AppUserId, r.Status })
            .HasDatabaseName("IX_Reservations_AppUserId_Status");
        builder.HasIndex(r => new { r.Status, r.PaymentStatus })
            .HasDatabaseName("IX_Reservations_Status_PaymentStatus");
        builder.HasIndex(r => r.CreatedDate)
            .HasDatabaseName("IX_Reservations_CreatedDate");

        //---Iliskiler---//
        builder.HasOne(r => r.AppUser)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.AppUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Reservations_AspNetUsers_AppUserId");

        builder.HasMany(r => r.Tickets)
            .WithOne(t => t.Reservation)
            .HasForeignKey(t => t.ReservationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Reservations_ReservationId");
        builder.Navigation(r => r.Tickets).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Payments)
            .WithOne(p => p.Reservation)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Payments_Reservations_ReservationId");

        builder.HasOne(r => r.Hotel)
            .WithMany()
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Reservations_Hotels_HotelId");

        builder.HasOne(r => r.Car)
            .WithMany()
            .HasForeignKey(r => r.CarId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Reservations_Cars_CarId");

        builder.HasOne(r => r.Tour)
            .WithMany()
            .HasForeignKey(r => r.TourId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Reservations_Tours_TourId");

        builder.HasMany(r => r.Passengers)
            .WithMany()
            .UsingEntity(j => j.ToTable("ReservationPassengers"));
        builder.Navigation(r => r.Passengers).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

