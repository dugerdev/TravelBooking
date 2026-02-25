using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelBooking.Infrastructure.Configurations;

//---Payment entity icin Fluent API konfigurasyonlari---//
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        //---Tablo adi---//
        builder.ToTable("Payments");

        //---BaseEntity ortak alanlari---//
        builder.HasKey(p => p.Id);

        //---Odeme ozel alanlari---//
        builder.Property(p => p.TransactionId)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Islem kimligi");

        builder.Property(p => p.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Odeme durumu");

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Odeme yontemi");

        builder.Property(p => p.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasComment("Islem turu");

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(500)
            .HasComment("Hata mesaji");

        builder.Property(p => p.TransactionDate)
            .IsRequired()
            .HasComment("Islem tarihi");

        //---Money Value Object icin owned entity---//
        //---EF Core owned type'larda column name otomatik olusturulur: {PropertyName}_{SubPropertyName}---//
        builder.OwnsOne(p => p.TransactionAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TransactionAmount_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Islem tutari");

            money.Property(m => m.Currency)
                .HasColumnName("TransactionAmount_Currency")
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasComment("Para birimi");
        });

        //---Index'ler---//
        builder.HasIndex(p => p.ReservationId);
        builder.HasIndex(p => p.TransactionId);
        builder.HasIndex(p => p.PaymentStatus);
        builder.HasIndex(p => p.TransactionDate);

        //---Iliskiler---//
        builder.HasOne(p => p.Reservation)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Payments_Reservations_ReservationId");
    }
}

