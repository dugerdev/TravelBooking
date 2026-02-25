using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Passenger entity icin Fluent API konfigurasyonlari---//
public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Passengers");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(p => p.Id);

        //---Yolcu ozel alanlari---//
        builder.Property(p => p.PassengerFirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Yolcu adi");

        builder.Property(p => p.PassengerLastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Yolcu soyadi");

        builder.Property(p => p.NationalNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("T.C. Kimlik No veya Yabanci Kimlik No");

        builder.Property(p => p.PassportNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Pasaport No");

        builder.Property(p => p.DateOfBirth)
            .IsRequired()
            .HasComment("Dogum tarihi");

        builder.Property(p => p.PassengerType)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Yolcu Tipi (Yetiskin, Cocuk, Bebek)");

        //---Index'ler---//
        builder.HasIndex(p => p.NationalNumber);
        builder.HasIndex(p => p.PassportNumber);

        //---Iliskiler---//
        builder.HasMany(p => p.Tickets)
            .WithOne(t => t.Passenger)
            .HasForeignKey(t => t.PassengerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tickets_Passengers_PassengerId");
    }
}

