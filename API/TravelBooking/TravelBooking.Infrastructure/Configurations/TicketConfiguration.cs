using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Ticket entity icin Fluent API konfigurasyonlari---//
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Tickets");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(t => t.Id);

        //---Bilet ozel alanlari---//
        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("E-posta adresi");

        builder.Property(t => t.ContactPhoneNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Iletisim telefon numarasi");

        builder.Property(t => t.SeatClass)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Koltuk sinifi");

        builder.Property(t => t.SeatNumber)
            .HasMaxLength(10)
            .HasComment("Koltuk numarasi");

        builder.Property(t => t.BaggageOption)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Bagaj secenegi");

        builder.Property(t => t.TicketPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Bilet fiyati");

        builder.Property(t => t.BaggageFee)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Bagaj ucreti");

        builder.Property(t => t.TicketStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Bilet durumu");

        builder.Property(t => t.CancelledAt)
            .HasComment("Iptal edilme tarihi");

        //---Index'ler---//
        builder.HasIndex(t => t.FlightId);
        builder.HasIndex(t => t.ReservationId);
        builder.HasIndex(t => t.PassengerId);
        builder.HasIndex(t => t.TicketStatus);

        //---Iliskiler---//
        //---Flight ile iliski---//
        builder.HasOne(t => t.Flight)
            .WithMany(f => f.Tickets)
            .HasForeignKey(t => t.FlightId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tickets_Flights_FlightId");

        //---Reservation ile iliski---//
        builder.HasOne(t => t.Reservation)
            .WithMany(r => r.Tickets)
            .HasForeignKey(t => t.ReservationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Reservations_ReservationId");

        //---Passenger ile iliski---//
        builder.HasOne(t => t.Passenger)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PassengerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tickets_Passengers_PassengerId");
    }
}

